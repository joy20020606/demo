import asyncio
import logging
import os

from sim.profile import load_profile
from sim.server import Simulator


def main() -> None:
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(name)s %(message)s")
    logging.getLogger("pymodbus").setLevel(logging.WARNING)
    profile = load_profile(os.environ.get("SIM_PROFILE", "profiles/site_a.yaml"))
    host = os.environ.get("SIM_HOST", "0.0.0.0")
    port = int(os.environ.get("SIM_PORT", "5020"))
    try:
        asyncio.run(Simulator(profile).run(host, port))
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
