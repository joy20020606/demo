import asyncio
import logging
import time

import aiomqtt

from gw.config import Settings, SiteConfig, load_site_config
from gw.poller import poll_once
from gw.readers import build_reader

log = logging.getLogger("gw")

MQTT_RETRY_SECONDS = 5


async def run(config: SiteConfig, settings: Settings) -> None:
    reader = build_reader(config)
    try:
        while True:
            try:
                async with aiomqtt.Client(
                    settings.mqtt_host,
                    settings.mqtt_port,
                    username=settings.mqtt_username,
                    password=settings.mqtt_password,
                    identifier=f"gateway-{config.site_id}",
                ) as mqtt:
                    log.info("connected to broker %s:%s", settings.mqtt_host, settings.mqtt_port)

                    async def publish(topic: str, payload: str, mqtt=mqtt) -> None:
                        await mqtt.publish(topic, payload, qos=1)

                    while True:
                        started = time.monotonic()
                        count = await poll_once(config.site_id, reader, publish)
                        log.info("published %s readings", count)
                        elapsed = time.monotonic() - started
                        await asyncio.sleep(max(0, config.poll_interval_seconds - elapsed))
            except aiomqtt.MqttError as exc:
                log.warning("broker unavailable: %s; retrying in %ss", exc, MQTT_RETRY_SECONDS)
                await asyncio.sleep(MQTT_RETRY_SECONDS)
    finally:
        await reader.close()


def main() -> None:
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(name)s %(message)s")
    logging.getLogger("pymodbus").setLevel(logging.ERROR)
    settings = Settings()
    config = load_site_config(settings.site_config)
    if settings.device_host:
        config = config.model_copy(update={"host": settings.device_host})
    if settings.device_port:
        config = config.model_copy(update={"port": settings.device_port})
    log.info(
        "gateway %s: %s points from %s:%s every %ss",
        config.site_id,
        len(config.points),
        config.host,
        config.port,
        config.poll_interval_seconds,
    )
    try:
        asyncio.run(run(config, settings), loop_factory=asyncio.SelectorEventLoop)
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
