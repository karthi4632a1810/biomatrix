"""
Fetch biometric punch data for a specific dwEnrollNumber from SQL Server
and save it to a new CSV file.

Install the only dependency once with:
    venv/bin/pip install python-tds
"""

import csv
import sys
from datetime import datetime

import pytds

# Fixed connection details (same as form.cs)
SERVER_IP = "192.168.103.33"
DATABASE = "Main_Biometric"
USERNAME = "mapims"
PASSWORD = "Biometric@2023"
TABLE = "punchtimedetails"
DEFAULT_LIMIT = 50


def get_connection():
    return pytds.connect(
        dsn=SERVER_IP,
        database=DATABASE,
        user=USERNAME,
        password=PASSWORD,
    )


def fetch_rows(conn, enroll_number: str, limit: int):
    with conn.cursor() as cur:
        cur.execute(
            f"SELECT TOP ({limit}) * FROM {TABLE} "
            f"WHERE dwEnrollNumber = %s ORDER BY id DESC",
            (enroll_number,),
        )
        columns = [col[0] for col in cur.description]
        rows = cur.fetchall()
    # reverse so the CSV reads oldest -> newest, matching the DB order
    rows.reverse()
    return columns, rows


def write_csv(columns, rows, out_path: str):
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(columns)
        writer.writerows(rows)


def main():
    enroll_number = input("Enter dwEnrollNumber to fetch: ").strip()
    if not enroll_number:
        print("No dwEnrollNumber entered, aborting.")
        sys.exit(1)

    limit_input = input(f"Enter row limit (default {DEFAULT_LIMIT}): ").strip()
    limit = int(limit_input) if limit_input else DEFAULT_LIMIT

    try:
        conn = get_connection()
    except Exception as exc:
        print(f"Failed to connect to {SERVER_IP}: {exc!r}")
        sys.exit(1)

    try:
        columns, rows = fetch_rows(conn, enroll_number, limit)
    finally:
        conn.close()

    if not rows:
        print(f"No rows found for dwEnrollNumber '{enroll_number}'.")
        return

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    out_path = f"biometric_{enroll_number}_{limit}_{timestamp}.csv"
    write_csv(columns, rows, out_path)
    print(f"Wrote {len(rows)} rows to {out_path}")


if __name__ == "__main__":
    main()
