"""
Loop through multiple biometric devices (ZK protocol) and fetch attendance
data directly from each one, saving everything into a single combined CSV.

Install the only dependency once with:
    venv/bin/pip install pyzk
"""

import csv
import sys
from datetime import datetime, time

from zk import ZK

DEVICES = [
    {"id": 1, "device_name": "AZ-DR_DEVICE New", "ip_address": "192.168.101.247", "port_no": 4370},
    {"id": 2, "device_name": "AZ-KIOSK-L", "ip_address": "192.168.103.244", "port_no": 4370},
    {"id": 3, "device_name": "CZ-DR_DEVICE-L", "ip_address": "192.168.103.209", "port_no": 4370},
    {"id": 4, "device_name": "CZ-DR_FACE READER-L", "ip_address": "192.168.100.250", "port_no": 4370},
    {"id": 5, "device_name": "CZ-FACE READER 1", "ip_address": "192.168.100.99", "port_no": 4370},
    {"id": 6, "device_name": "CZ-EMP_DEVICE2", "ip_address": "192.168.103.19", "port_no": 4370},
    {"id": 7, "device_name": "CZ-KIOSK-L", "ip_address": "192.168.100.252", "port_no": 4370},
    {"id": 8, "device_name": "CZ-FACE DEVICE NEW", "ip_address": "192.168.103.119", "port_no": 4370},
    {"id": 9, "device_name": "AZ-DR_DEVICE", "ip_address": "192.168.103.117", "port_no": 4370},
    {"id": 10, "device_name": "CZ-FACE READER NEW", "ip_address": "192.168.101.8", "port_no": 4370},
    {"id": 11, "device_name": "CZ-FACE READER 2", "ip_address": "192.168.101.4", "port_no": 4370},
]
CONNECT_TIMEOUT = 10
DEFAULT_LIMIT = 50
DATE_FORMAT = "%d-%m-%Y"


def parse_date(label: str):
    raw = input(f"{label} (DD-MM-YYYY, blank = no limit): ").strip()
    if not raw:
        return None
    return datetime.strptime(raw, DATE_FORMAT)


def fetch_from_device(ip: str, port: int, user_id: str, limit: int, from_date, to_date):
    zk = ZK(ip, port=port, timeout=CONNECT_TIMEOUT)
    conn = zk.connect()
    try:
        records = conn.get_attendance()
        names = {u.user_id: u.name for u in conn.get_users()}
    finally:
        conn.disconnect()

    if user_id:
        records = [r for r in records if r.user_id == user_id]
    if from_date:
        records = [r for r in records if r.timestamp >= from_date]
    if to_date:
        end_of_day = datetime.combine(to_date.date(), time(23, 59, 59))
        records = [r for r in records if r.timestamp <= end_of_day]

    records.sort(key=lambda r: r.timestamp)
    return records[-limit:], names


def write_csv(rows, out_path: str):
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(
            ["device_name", "device_ip", "uid", "user_id", "name", "timestamp", "status", "punch"]
        )
        writer.writerows(rows)


def main():
    user_id = input("Enter user_id to filter by (blank = all users): ").strip()

    limit_input = input(f"Enter row limit per device (default {DEFAULT_LIMIT}): ").strip()
    limit = int(limit_input) if limit_input else DEFAULT_LIMIT

    from_date = parse_date("From date")
    to_date = parse_date("To date")

    all_rows = []
    for device in DEVICES:
        ip = device["ip_address"]
        name = device["device_name"]
        print(f"Connecting to {name} ({ip}) ...")
        try:
            records, names = fetch_from_device(
                ip, device["port_no"], user_id, limit, from_date, to_date
            )
        except Exception as exc:
            print(f"  Skipped {name} ({ip}): {exc!r}")
            continue
        print(f"  Got {len(records)} rows from {name} ({ip})")
        for r in records:
            all_rows.append(
                [name, ip, r.uid, r.user_id, names.get(r.user_id, ""), r.timestamp, r.status, r.punch]
            )

    if not all_rows:
        print("No records fetched from any device.")
        return

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    out_path = f"device_all_devices_{timestamp}.csv"
    write_csv(all_rows, out_path)
    print(f"Wrote {len(all_rows)} rows total to {out_path}")


if __name__ == "__main__":
    main()
