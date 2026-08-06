import { useEffect, useRef, useState } from "react";
import { useParams } from "react-router-dom";

const REFRESH_MS = 10000;
const API_BASE = `${window.location.protocol}//${window.location.hostname}:7000`;

const AVATAR_COLORS = [
  "#f97066",
  "#f79009",
  "#fdb022",
  "#84cc16",
  "#22c55e",
  "#10b981",
  "#06b6d4",
  "#3b82f6",
  "#6366f1",
  "#a855f7",
  "#ec4899",
];

function colorForId(id) {
  let hash = 0;
  for (const ch of String(id)) hash = (hash * 31 + ch.charCodeAt(0)) >>> 0;
  return AVATAR_COLORS[hash % AVATAR_COLORS.length];
}

function initials(id) {
  const s = String(id);
  return s.length <= 2 ? s : s.slice(-2);
}

function formatTime(iso) {
  const d = new Date(iso);
  return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
}

function timeAgo(iso, now) {
  const diffSec = Math.max(0, Math.floor((now - new Date(iso)) / 1000));
  if (diffSec < 60) return "just now";
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin} min ago`;
  const diffHr = Math.floor(diffMin / 60);
  return `${diffHr} hr ago`;
}

function PunchCard({ entry, isNewest, now }) {
  const [imgFailed, setImgFailed] = useState(false);

  return (
    <div className={`card${isNewest ? " card--newest" : ""}`}>
      {isNewest && <div className="card__badge">Just Punched</div>}
      <div className="card__avatar">
        {!imgFailed ? (
          <img
            src={entry.photo}
            alt={entry.name}
            onError={() => setImgFailed(true)}
          />
        ) : (
          <div
            className="card__avatar-fallback"
            style={{ background: colorForId(entry.user_id) }}
          >
            {initials(entry.user_id)}
          </div>
        )}
      </div>
      <div className="card__name">{entry.name}</div>
      <div className="card__id">ID {entry.user_id}</div>
      <div className="card__time">{formatTime(entry.punches[0])}</div>
      <div className="card__ago">{timeAgo(entry.punches[0], now)}</div>
      {entry.punches.length > 1 && (
        <div className="card__history">
          {entry.punches.slice(1).map((p) => (
            <span key={p} className="card__history-chip">
              {formatTime(p)}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}

export default function DisplayScreen() {
  const { ip, limit } = useParams();
  const [entries, setEntries] = useState([]);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);
  const [now, setNow] = useState(new Date());
  const [lastUpdated, setLastUpdated] = useState(null);
  const timerRef = useRef(null);

  useEffect(() => {
    const clockId = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(clockId);
  }, []);

  useEffect(() => {
    let cancelled = false;

    async function fetchData() {
      try {
        const res = await fetch(`${API_BASE}/api/attendance/${ip}/${limit}`);
        const body = await res.json();
        if (cancelled) return;
        if (!res.ok) {
          setError(body.error || "Failed to fetch device data");
        } else {
          setEntries(body.data);
          setError(null);
          setLastUpdated(new Date());
        }
      } catch (err) {
        if (!cancelled) setError(String(err.message || err));
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchData();
    timerRef.current = setInterval(fetchData, REFRESH_MS);
    return () => {
      cancelled = true;
      clearInterval(timerRef.current);
    };
  }, [ip, limit]);

  return (
    <div className="screen">
      <header className="header">
        <div className="header__brand">Attendance Display</div>
        <div className="header__clock">
          {now.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" })}
          <span className="header__date">
            {now.toLocaleDateString([], { weekday: "long", year: "numeric", month: "long", day: "numeric" })}
          </span>
        </div>
      </header>

      <div className="statusbar">
        <span className="statusbar__dot" />
        <span>
          Device {ip} &middot; refreshing every 10s
          {lastUpdated && <> &middot; last updated {lastUpdated.toLocaleTimeString()}</>}
        </span>
      </div>

      {loading && <div className="center-message">Connecting to device...</div>}

      {!loading && error && (
        <div className="center-message center-message--error">
          Unable to reach device {ip}. Retrying...
          <div className="center-message__detail">{error}</div>
        </div>
      )}

      {!loading && !error && entries.length === 0 && (
        <div className="center-message">No punches recorded yet.</div>
      )}

      {!loading && !error && entries.length > 0 && (
        <div
          className="grid"
          style={{
            gridTemplateColumns: `repeat(${
              entries.length <= 6 ? entries.length : Math.ceil(entries.length / 2)
            }, 1fr)`,
          }}
        >
          {entries.map((entry, i) => (
            <PunchCard
              key={`${entry.user_id}-${entry.punches[0]}-${i}`}
              entry={entry}
              isNewest={i === 0}
              now={now}
            />
          ))}
        </div>
      )}
    </div>
  );
}
