import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import "./index.css";
import DisplayScreen from "./DisplayScreen.jsx";
import Home from "./Home.jsx";

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/:ip/:limit" element={<DisplayScreen />} />
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
