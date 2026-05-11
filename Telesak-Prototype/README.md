# إسناد / Telesak — Client Demo Prototype

A polished, self-contained marketing prototype of the **E-Esnad / Telesak** task management platform — built specifically for **client presentations**.

Pure static HTML/CSS/JS. Zero backend. Deploys in 60 seconds on any free static host.

---

## 🚀 Quick deploy (pick one)

### Option 1 — Netlify Drop (fastest, no account needed)
1. Open https://app.netlify.com/drop in your browser
2. Drag the **whole `Telesak-Prototype` folder** onto the page
3. You get a live URL in ~30 seconds: `https://random-name.netlify.app`
4. Share that URL with the client

### Option 2 — Vercel
```bash
npm i -g vercel
cd Telesak-Prototype
vercel
```

### Option 3 — GitHub Pages
1. Create a new repo on GitHub (e.g. `esnad-demo`)
2. Push this folder:
   ```bash
   cd Telesak-Prototype
   git init && git add . && git commit -m "demo"
   git branch -M main
   git remote add origin https://github.com/<you>/esnad-demo.git
   git push -u origin main
   ```
3. Repo → Settings → Pages → Source: `main` / `(root)` → Save
4. URL appears in ~60 seconds: `https://<you>.github.io/esnad-demo/`

### Option 4 — Cloudflare Pages
- Connect the GitHub repo at https://pages.cloudflare.com — auto-deploys on every push.

---

## 🔑 Demo credentials

The login screen has a **"حسابات تجريبية"** card — clicking any account auto-fills the form.

| Role | Domain | Username | Password | What they see |
|------|--------|----------|----------|---------------|
| 🟦 **Manager (المشرف)** | `esnad` | `admin` | `admin123` | Full admin dashboard, all 10 pages |
| 🟢 Employee 1 (مطور) | `esnad` | `ahmed` | `123456` | Employee dashboard, "My tasks" view |
| 🟢 Employee 2 (مصممة) | `esnad` | `sara` | `123456` | Employee view (different data) |
| 🟢 Employee 3 (محللة) | `esnad` | `mona` | `123456` | Employee view |
| 🟢 Employee 4 (Backend) | `esnad` | `yousef` | `123456` | Employee view |
| 🔴 **Stopped account** | `esnad` | `khalid` | `123456` | Shows "تم إيقاف حسابك" page |

> The stopped-account flow demonstrates the access-control logic — useful for the security part of the demo.

---

## 🎬 Suggested 15-minute demo script

### 0. Open the URL → Login screen (1 min)
- Point out the **3-field login**: Domain + Username + Password (multi-tenant model)
- Click the **"admin"** demo card → form auto-fills → click **دخول**
- Mention: "Each company gets its own subdomain — `<company>.telesak.app`"

### 1. Manager Dashboard (3 min) — *the wow moment*
- 4 KPI cards with live mini-trends
- Donut chart: task status distribution
- Activity feed: real-time updates
- Productivity bar chart: top performers
- Upcoming deadlines table
- Monthly performance bar chart
- Toggle **هجري/ميلادي** in the topbar — every date updates

### 2. Projects (1 min)
- 5 project cards with progress bars
- Click "تطوير منصة إسناد ٢.٠" → drill into project detail
- Project's own KPIs + scoped task list

### 3. Tasks Kanban (3 min) — *the "I get it" moment*
- 4 columns: New → In Progress → Done → Approved
- **Drag a card** between columns → toast confirmation, status updates
- Click "🔄 عرض جدولي" to switch to table view
- Use filters (project / employee / priority / search) — all live
- Click **"+ مهمة جديدة"** → create a new task in 5 seconds → it appears in the right column

### 4. Task detail drawer (2 min)
- Click any task card → side drawer slides in
- Show: meta grid, attachments list, comments thread
- Add a new comment → appears immediately
- Show the workflow buttons:
  - Employee view: **قبول / رفض** for new tasks, **إنهاء** for in-progress
  - Manager view: **اعتماد / رفض الإنجاز** for completed tasks

### 5. Employees (1 min)
- Table with 5 employees, productivity stats inline
- Toggle one employee to "موقوف" — toast appears
- Click **"+ إضافة موظف"** → form modal → save

### 6. Reports (2 min) — *the C-level value*
- KPIs + 2 charts + monthly bar
- Toggle Hijri calendar — report dates update
- Click **"🖨 طباعة / PDF"** → browser print preview opens with a clean report layout

### 7. Activity log + Attendance (1 min)
- Activity log: full audit trail (timeline)
- Attendance: 7 days of check-in/check-out

### 8. Switch to Employee role (1.5 min)
- Sidebar → Logout
- Click the **"ahmed"** demo card → log in
- Different dashboard: "My tasks today", attendance status, my productivity sparkline
- Open a task → click **قبول** → it moves to "جاري العمل"
- Click **إنهاء** on an in-progress task → moves to "منتهية"

### 9. Stopped account (30 sec) — *the security flourish*
- Logout
- Click **"khalid"** demo card → submit
- Full-screen "تم إيقاف حسابك" page

**Total: ~15 minutes.**

---

## 📦 What's inside

```
Telesak-Prototype/
├── index.html          # Login (split-screen hero + form)
├── app.html            # SPA shell (sidebar + topbar + main)
├── css/
│   └── styles.css      # Design system, components, responsive
├── js/
│   ├── data.js         # Seed data + Store/Session helpers
│   ├── auth.js         # Login flow + login-mode chat
│   ├── charts.js       # Pure-SVG donut/bar/spark/vbar charts
│   ├── chat.js         # Floating assistant (full mode in app)
│   └── app.js          # Router + 13 views
└── README.md           # This file
```

## 🎨 Visual direction

**Enterprise Polished (B)** — Telesak blue (`#3d85c6`) elevated:
- Modern card-based UI with soft shadows
- Cairo font (Google Fonts)
- RTL Arabic throughout
- Pure-SVG charts (no Chart.js dependency)
- Smooth 180–300ms transitions
- Status chips with proper color semantics
- Print-optimized report mode

## 🧰 Pre-loaded data

- **1 company** — شركة الإسناد للتقنية
- **5 employees** (4 active + 1 stopped) with realistic Saudi names
- **5 projects** at different stages (active / done / planning)
- **25 tasks** distributed across all statuses, priorities, deadlines
- **7 comments** on selected tasks
- **12 activity log entries**
- **7 days** of attendance records
- **5 notifications** in the bell dropdown

## ✅ Features demonstrated

| Feature | Where to see it |
|---------|-----------------|
| 3-field login (domain + user + pass) | `index.html` |
| Demo credentials autofill | Login card |
| Stopped-account flow | Login as `khalid` |
| Role-based UI (admin vs employee) | Sidebar + dashboard differ |
| KPI cards with sparklines | Dashboard |
| Donut + bar + monthly charts | Dashboard, Reports |
| Kanban with drag-and-drop | Tasks page |
| Filters (project, employee, priority, search) | Tasks page |
| Task detail drawer with comments + attachments | Click any task |
| Task workflow (accept/reject/finish/approve) | Drawer footer |
| Project drill-down | Click any project card |
| Add task / Add employee modals | + buttons |
| Employee toggle (active/stopped) | Employees page |
| Hijri / Gregorian calendar toggle | Topbar |
| Print to PDF (clean layout) | Reports page |
| Activity log timeline | Activity page |
| Attendance records | Attendance page |
| Notifications dropdown | Topbar bell |
| Floating chat assistant | Bottom-left button |
| Toast feedback on every action | Drag/save/toggle |
| Reset demo data | Topbar reset button |

## 🛠️ Customization for the client

Want to brand it for the specific client?

- **Company name** — edit `js/data.js` → `SEED.company.name`
- **Demo accounts** — edit `SEED.accounts`
- **Projects / tasks** — edit `SEED.projects`, `SEED.tasks`
- **Brand color** — edit `css/styles.css` → `:root { --primary: #...; }`
- **Logo** — replace the inline SVG in `index.html` and `app.html`

After editing, **clear browser localStorage** to reseed (or click the reset button in the topbar).

## 🌐 Browser support

Chrome, Edge, Firefox, Safari (any recent version). Hijri calendar uses native `Intl` — supported everywhere.

## 📝 Notes

- All data lives in `localStorage` per browser — each client viewing the URL gets their own independent demo
- The reset button restores the seed data
- No analytics, no external API calls except Google Fonts
- The "AI assistant" chat uses local Q&A matching (no real Claude API in the demo)

---

**Questions?** This prototype intentionally simplifies the real Telesak architecture (ASP.NET MVC + SQL Server) into a static site so it can be shared anywhere. The full app is the production system; this is the showcase.
