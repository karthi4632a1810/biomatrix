"""
Fetch attendance/punch data directly from a biometric device (ZK protocol)
and save it to a new CSV file - no SQL database involved.

Install the only dependency once with:
    venv/bin/pip install pyzk
"""

import csv
import sys
from datetime import datetime, time

from zk import ZK

# Fixed device details
DEVICE_IP = "192.168.101.247"  # AZ-DR_DEVICE
DEVICE_PORT = 4370
DEFAULT_LIMIT = 50
DATE_FORMAT = "%d-%m-%Y"


def get_connection():
    zk = ZK(DEVICE_IP, port=DEVICE_PORT, timeout=10)
    return zk.connect()


def parse_date(label: str):
    raw = input(f"{label} (DD-MM-YYYY, blank = no limit): ").strip()
    if not raw:
        return None
    return datetime.strptime(raw, DATE_FORMAT)


def fetch_records(conn, user_id: str, limit: int, from_date, to_date):
    records = conn.get_attendance()
    names = {u.user_id: u.name for u in conn.get_users()}
    if user_id:
        records = [r for r in records if r.user_id == user_id]
    if from_date:
        records = [r for r in records if r.timestamp >= from_date]
    if to_date:
        end_of_day = datetime.combine(to_date.date(), time(23, 59, 59))
        records = [r for r in records if r.timestamp <= end_of_day]
    records.sort(key=lambda r: r.timestamp)
    return records[-limit:], names


def write_csv(records, names, out_path: str):
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(["uid", "user_id", "name", "timestamp", "status", "punch"])
        for r in records:
            writer.writerow(
                [r.uid, r.user_id, names.get(r.user_id, ""), r.timestamp, r.status, r.punch]
            )


def main():
    user_id = input("Enter user_id to filter by (blank = all users): ").strip()

    limit_input = input(f"Enter row limit (default {DEFAULT_LIMIT}): ").strip()
    limit = int(limit_input) if limit_input else DEFAULT_LIMIT

    from_date = parse_date("From date")
    to_date = parse_date("To date")

    try:
        conn = get_connection()
    except Exception as exc:
        print(f"Failed to connect to {DEVICE_IP}:{DEVICE_PORT}: {exc!r}")
        sys.exit(1)

    try:
        records, names = fetch_records(conn, user_id, limit, from_date, to_date)
    finally:
        conn.disconnect()

    if not records:
        print("No matching records found.")
        return

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    suffix = user_id if user_id else "all"
    out_path = f"device_{suffix}_{limit}_{timestamp}.csv"
    write_csv(records, names, out_path)
    print(f"Wrote {len(records)} rows to {out_path}")


if __name__ == "__main__":
    main()
