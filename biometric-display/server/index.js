const express = require("express");
const cors = require("cors");
const ZKLib = require("node-zklib");

const app = express();
app.use(cors());

const DEVICE_PORT = 4370;
const CONNECT_TIMEOUT = 10000;
const INPORT = 4000;

app.get("/api/attendance/:ip/:limit", async (req, res) => {
  const { ip, limit } = req.params;
  const wantedCount = Math.max(1, parseInt(limit, 10) || 12);

  const zk = new ZKLib(ip, DEVICE_PORT, CONNECT_TIMEOUT, INPORT);

  try {
    await zk.createSocket();

    const usersResult = await zk.getUsers();
    const nameByUserId = {};
    for (const u of usersResult.data || []) {
      nameByUserId[u.userId] = u.name || u.userId;
    }

    const logsResult = await zk.getAttendances();
    const records = logsResult.data || [];

    // chronological order, oldest first, so consecutive same-user punches group correctly
    records.sort((a, b) => new Date(a.recordTime) - new Date(b.recordTime));

    // collapse consecutive punches from the same user into one "run" (card).
    // a user punching again after someone else punched in between starts a new run.
    const runs = [];
    for (const r of records) {
      const last = runs[runs.length - 1];
      if (last && last.user_id === r.deviceUserId) {
        last.punches.push(r.recordTime);
      } else {
        runs.push({ user_id: r.deviceUserId, punches: [r.recordTime] });
      }
    }

    const latestRuns = runs.slice(-wantedCount).reverse();

    const cards = latestRuns.map((run) => ({
      user_id: run.user_id,
      name: nameByUserId[run.user_id] || run.user_id,
      photo: `https://mapims.online/files/staff_idcard/${run.user_id}.png`,
      punches: run.punches.slice(-3).reverse(),
    }));

    res.json({ ip, count: cards.length, data: cards });
  } catch (err) {
    res.status(502).json({ error: String(err && err.message ? err.message : err) });
  } finally {
    try {
      await zk.disconnect();
    } catch (_) {}
  }
});

const PORT = process.env.PORT || 7000;
app.listen(PORT, () => console.log(`Server listening on port ${PORT}`));
