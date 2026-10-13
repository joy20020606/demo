import argparse
import sys
from pathlib import Path

from pymodbus.client import ModbusTcpClient

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from sim.profile import load_profile  # noqa: E402
from sim.registers import decode, encode  # noqa: E402

FAULTS = ("sensor_stuck", "scale_error")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--profile", default="profiles/site_a.yaml")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=5020)
    parser.add_argument("--set", nargs=2, metavar=("NAME", "VALUE"))
    parser.add_argument("--fault", nargs=2, metavar=("NAME", "on|off"))
    args = parser.parse_args()

    profile = load_profile(args.profile)
    client = ModbusTcpClient(args.host, port=args.port)
    if not client.connect():
        sys.exit(f"cannot connect to {args.host}:{args.port}")

    try:
        if args.set:
            name, value = args.set
            register = profile.register(name)
            client.write_registers(
                register.address,
                encode(float(value), register.scale, register.words),
                device_id=profile.device_id,
            )
        if args.fault:
            name, state = args.fault
            if name not in FAULTS:
                sys.exit(f"unknown fault, choose from {FAULTS}")
            client.write_coil(
                getattr(profile.faults, name), state == "on", device_id=profile.device_id
            )

        print(f"{'name':<18}{'address':>8}{'raw':>16}{'value':>12}")
        for register in profile.registers:
            result = client.read_holding_registers(
                register.address, count=register.words, device_id=profile.device_id
            )
            raw = list(result.registers)
            value = decode(raw, register.scale)
            print(f"{register.name:<18}{register.address:>8}{str(raw):>16}{value:>12.2f}")
    finally:
        client.close()


if __name__ == "__main__":
    main()
