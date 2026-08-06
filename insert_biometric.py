"""
Insert a new punch record into punchtimedetails in the Main_Biometric database.

Install the only dependency once with:
    venv/bin/pip install python-tds
"""

import sys
from datetime import datetime

import pytds

# Fixed connection details (same as form.cs)
SERVER_IP = "192.168.103.244"
DATABASE = "Main_Biometric"
USERNAME = "mapims"
PASSWORD = "Biometric@2023"
TABLE = "punchtimedetails"


def get_connection():
    return pytds.connect(
        dsn=SERVER_IP,
        database=DATABASE,
        user=USERNAME,
        password=PASSWORD,
    )


def prompt(label: str, default: str = "") -> str:
    suffix = f" (default {default})" if default else ""
    value = input(f"{label}{suffix}: ").strip()
    return value or default


def insert_row(conn, values: dict):
    columns = ", ".join(values.keys())
    placeholders = ", ".join(["%s"] * len(values))
    with conn.cursor() as cur:
        cur.execute(
            f"INSERT INTO {TABLE} ({columns}) OUTPUT INSERTED.ID "
            f"VALUES ({placeholders})",
            tuple(values.values()),
        )
        new_id = cur.fetchone()[0]
    conn.commit()
    return new_id


def main():
    now = datetime.now()

    dw_enroll_number = prompt("dwEnrollNumber")
    if not dw_enroll_number:
        print("dwEnrollNumber is required, aborting.")
        sys.exit(1)

    values = {
        "dwEnrollNumber": dw_enroll_number,
        "dwTMachineNumber": prompt("dwTMachineNumber"),
        "dwEMachineNumber": prompt("dwEMachineNumber"),
        "dwVerifyMode": prompt("dwVerifyMode"),
        "punch_date": prompt("punch_date (YYYY-MM-DD)", now.strftime("%Y-%m-%d")),
        "punch_time": prompt("punch_time (HH:MM:SS)", now.strftime("%H:%M:%S")),
        "status": int(prompt("status", "0")),
        "device_name": prompt("device_name"),
        "device_id": prompt("device_id"),
        "ipaddress": prompt("ipaddress"),
    }

    try:
        conn = get_connection()
    except Exception as exc:
        print(f"Failed to connect to {SERVER_IP}: {exc!r}")
        sys.exit(1)

    try:
        new_id = insert_row(conn, values)
    except Exception as exc:
        print(f"Insert failed: {exc!r}")
        sys.exit(1)
    finally:
        conn.close()

    print(f"Inserted new row with ID = {new_id}")


if __name__ == "__main__":
    main()
