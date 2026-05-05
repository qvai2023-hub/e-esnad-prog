/* ========================================================================
   إسناد / Telesak — SPA router + views
   ======================================================================== */

(function () {
  'use strict';

  // ============== Boot: require session ==============
  const session = Session.get();
  if (!session) { location.href = 'index.html'; return; }

  let state = Store.load();
  let calendar = 'greg';
  let activeFilter = { status: 'all', project: 'all', emp: 'all', q: '' };

  // ============== UI references ==============
  const main = document.getElementById('appMain');
  const sbNav = document.getElementById('sbNav');
  const sbUser = document.getElementById('sbUser');
  const sbUserRole = document.getElementById('sbUserRole');
  const sbAvatar = document.getElementById('sbAvatar');
  const sbCompany = document.getElementById('sbCompany');
  const bcrumb = document.getElementById('bcrumb');

  // ============== Init shell ==============
  sbCompany.textContent = state.company.name;
  sbUser.textContent = session.name;
  sbUserRole.textContent = session.title;
  sbAvatar.textContent = session.name.split(' ').map(s=>s[0]).slice(0,2).join('');
  sbAvatar.className = `avatar av-${session.avatar || 1}`;

  buildNav();
  loadNotifs();
  bindShellEvents();
  navigate(session.role === 'admin' ? 'dashboard' : 'emp-dashboard');

  // ============== NAV ==============
  function buildNav() {
    const adminNav = [
      { id: 'dashboard',  i: '📊', t: 'لوحة التحكم' },
      { id: 'tasks',      i: '📋', t: 'المهام', badge: pendingCount() },
      { id: 'projects',   i: '📁', t: 'المشاريع' },
      { id: 'employees',  i: '👥', t: 'الموظفين' },
      { type: 'sep', label: 'تقارير وتحليلات' },
      { id: 'reports',    i: '📈', t: 'التقارير' },
      { id: 'attendance', i: '🕐', t: 'الحضور والانصراف' },
      { id: 'activity',   i: '📜', t: 'سجل العمليات' },
      { type: 'sep', label: 'الحساب' },
      { id: 'profile',    i: '👤', t: 'الملف الشخصي' },
      { id: 'help',       i: '❔', t: 'المساعدة' }
    ];
    const empNav = [
      { id: 'emp-dashboard', i: '🏠', t: 'الرئيسية' },
      { id: 'emp-tasks',     i: '📋', t: 'مهامي', badge: myPendingCount() },
      { id: 'emp-reports',   i: '📈', t: 'تقاريري' },
      { id: 'emp-attendance', i: '🕐', t: 'حضوري' },
      { type: 'sep', label: 'الحساب' },
      { id: 'profile',       i: '👤', t: 'الملف الشخصي' },
      { id: 'help',          i: '❔', t: 'المساعدة' }
    ];
    const items = session.role === 'admin' ? adminNav : empNav;

    sbNav.innerHTML = items.map(it => {
      if (it.type === 'sep') return `<div class="sidebar__group-label">${it.label}</div>`;
      const badge = it.badge ? `<span class="nav-item__badge">${it.badge}</span>` : '';
      return `<a class="nav-item" data-route="${it.id}" href="#${it.id}"><span class="nav-item__icon">${it.i}</span>${it.t}${badge}</a>`;
    }).join('');

    sbNav.addEventListener('click', e => {
      const a = e.target.closest('[data-route]');
      if (!a) return;
      e.preventDefault();
      navigate(a.dataset.route);
    });
  }

  function pendingCount() {
    return state.tasks.filter(t => t.status === 'new' || t.status === 'progress' || t.status === 'late').length;
  }
  function myPendingCount() {
    return state.tasks.filter(t => t.empId === session.empId && (t.status === 'new' || t.status === 'progress' || t.status === 'late')).length;
  }

  // ============== ROUTER ==============
  function navigate(route, ctx) {
    document.querySelectorAll('.nav-item').forEach(a => a.classList.toggle('nav-item--active', a.dataset.route === route));
    location.hash = route + (ctx ? `/${ctx}` : '');

    const routes = {
      dashboard:     viewAdminDashboard,
      tasks:         viewTasks,
      projects:      viewProjects,
      'project-detail': () => viewProjectDetail(ctx),
      employees:     viewEmployees,
      reports:       viewReports,
      attendance:    viewAttendance,
      activity:      viewActivity,
      profile:       viewProfile,
      help:          viewHelp,
      'emp-dashboard': viewEmpDashboard,
      'emp-tasks':     viewEmpTasks,
      'emp-reports':   viewEmpReports,
      'emp-attendance': viewEmpAttendance
    };
    const fn = routes[route] || routes.dashboard;
    fn();
    main.scrollTo({ top: 0 });
  }

  window.navigate = navigate;

  // ============== ADMIN DASHBOARD ==============
  function viewAdminDashboard() {
    bcrumb.innerHTML = `<b>لوحة التحكم</b>`;
    const t = state.tasks;
    const total = t.length;
    const inProgress = t.filter(x => x.status === 'progress').length;
    const done = t.filter(x => x.status === 'done' || x.status === 'approved').length;
    const late = t.filter(x => x.status === 'late').length;

    main.innerHTML = `
      <div class="page-head">
        <div>
          <h1>أهلاً بعودتك، ${session.name.split(' ')[0]} 👋</h1>
          <p>إليك نظرة سريعة على أداء الفريق اليوم</p>
        </div>
        <div class="page-head__actions">
          <button class="btn btn--ghost" onclick="navigate('reports')">📊 التقارير</button>
          <button class="btn btn--primary" onclick="navigate('tasks')">+ مهمة جديدة</button>
        </div>
      </div>

      <!-- KPIs -->
      <div class="kpi-grid">
        ${kpiCard('إجمالي المهام', total, 'blue', 'up', '+8%', '📋', [12,14,13,16,18,17,20,22])}
        ${kpiCard('قيد التنفيذ', inProgress, 'amber', 'up', '+3', '⏱', [4,5,6,5,7,8,7,8])}
        ${kpiCard('منجزة', done, 'green', 'up', '+5', '✓', [10,12,11,14,15,17,18,20])}
        ${kpiCard('متأخرة', late, 'red', 'down', '-1', '⚠', [3,4,3,2,2,1,2,1])}
      </div>

      <!-- Charts row -->
      <div class="row row--2 mb-4">
        <div class="card">
          <div class="card__head">
            <div>
              <h3>توزيع المهام حسب الحالة</h3>
              <small>إجمالي ${total} مهمة عبر ${state.projects.length} مشاريع</small>
            </div>
          </div>
          <div class="card__body" id="donutWrap"></div>
        </div>
        <div class="card">
          <div class="card__head">
            <div>
              <h3>أبرز الأنشطة</h3>
              <small>آخر التحديثات على المهام والمشاريع</small>
            </div>
            <a href="#activity" onclick="navigate('activity'); return false;" class="text-sm">عرض الكل ←</a>
          </div>
          <div class="card__body">${activityList(state.activities.slice(0, 6))}</div>
        </div>
      </div>

      <!-- Productivity + upcoming -->
      <div class="row row--2 mb-4">
        <div class="card">
          <div class="card__head">
            <div>
              <h3>إنتاجية الفريق</h3>
              <small>المهام المنجزة هذا الشهر</small>
            </div>
          </div>
          <div class="card__body" id="prodWrap"></div>
        </div>

        <div class="card">
          <div class="card__head">
            <div>
              <h3>المهام القادمة</h3>
              <small>التسليمات في الأسبوع الحالي</small>
            </div>
          </div>
          <div class="card__body" style="padding: 0;">${upcomingList()}</div>
        </div>
      </div>

      <!-- Monthly bar -->
      <div class="card mb-4">
        <div class="card__head">
          <div>
            <h3>الأداء الشهري</h3>
            <small>عدد المهام المنجزة عبر آخر 6 أشهر</small>
          </div>
        </div>
        <div class="card__body" id="monthlyWrap"></div>
      </div>
    `;

    // donut
    const counts = ['new','progress','done','approved','late','paused','rejected'].map(s => ({
      label: STATUS_LABELS[s], value: t.filter(x=>x.status===s).length,
      color: ({new:'#3d85c6', progress:'#6366f1', done:'#10b981', approved:'#8b5cf6', late:'#ef4444', paused:'#f59e0b', rejected:'#6b7280'})[s]
    })).filter(d => d.value > 0);
    document.getElementById('donutWrap').innerHTML = Charts.donut(counts, { size: 220, label: 'مهمة' });

    // productivity bar
    const perEmp = state.accounts.filter(a => a.role === 'employee' && a.active).map(a => ({
      label: a.name.split(' ').slice(0,2).join(' '),
      value: t.filter(x => x.empId === a.empId && (x.status === 'done' || x.status === 'approved')).length,
      color: ({1:'#3d85c6',2:'#10b981',3:'#f59e0b',4:'#8b5cf6',5:'#ef4444',6:'#06b6d4'})[a.avatar]
    })).sort((a,b)=>b.value-a.value);
    const prodWrap = document.getElementById('prodWrap');
    prodWrap.innerHTML = Charts.bar(perEmp);
    Charts.animateBars(prodWrap);

    // monthly
    const monthly = [
      { label: 'ديسمبر', value: 18 }, { label: 'يناير', value: 22 }, { label: 'فبراير', value: 27 },
      { label: 'مارس', value: 31 }, { label: 'أبريل', value: 28 }, { label: 'مايو', value: done }
    ];
    document.getElementById('monthlyWrap').innerHTML = Charts.vbar(monthly, { h: 220 });
  }

  function kpiCard(label, value, color, dir, delta, icon, spark) {
    return `
      <div class="kpi">
        <div class="kpi__head">
          <div>
            <div class="kpi__label">${label}</div>
            <div class="kpi__value mt-2">${value}</div>
            <div class="kpi__delta kpi__delta--${dir}">
              ${dir==='up' ? '↑' : dir==='down' ? '↓' : '·'} ${delta}
            </div>
          </div>
          <div class="kpi__icon kpi__icon--${color}">${icon}</div>
        </div>
        <div class="kpi__spark">${Charts.spark(spark, { w: 220, h: 32, color: color==='red'?'#ef4444': color==='green'?'#10b981': color==='amber'?'#f59e0b':'#3d85c6' })}</div>
      </div>`;
  }

  function activityList(items) {
    return `
      <div class="activity">
        ${items.map(a => {
          const acc = state.accounts.find(x => x.empId === a.by);
          const name = acc ? acc.name.split(' ').slice(0,2).join(' ') : a.by;
          return `
            <div class="activity__item">
              <div class="activity__icon activity__icon--${a.color}">${a.icon}</div>
              <div class="flex-1">
                <div class="activity__text"><b>${name}</b> ${a.what}${a.target ? ' <b>«'+a.target+'»</b>' : ''}</div>
                <div class="activity__time">${a.date}</div>
              </div>
            </div>`;
        }).join('')}
      </div>`;
  }

  function upcomingList() {
    const today = new Date('2026-05-04');
    const week = new Date('2026-05-11');
    const upcoming = state.tasks
      .filter(t => t.status !== 'done' && t.status !== 'approved')
      .filter(t => { const d = new Date(t.due); return d >= today && d <= week; })
      .sort((a,b) => new Date(a.due) - new Date(b.due))
      .slice(0, 6);

    if (!upcoming.length) return `<div class="empty"><div class="empty__icon">🎉</div><b>لا توجد تسليمات هذا الأسبوع</b><p>الفريق متقدم على الجدول!</p></div>`;

    return `<table class="table table--clickable">
      <tbody>
        ${upcoming.map(t => {
          const emp = state.accounts.find(a => a.empId === t.empId);
          const proj = state.projects.find(p => p.id === t.projectId);
          return `<tr onclick="openTask('${t.id}')">
            <td>
              <div class="table__title">${t.title}</div>
              <small class="text-muted">${proj?.name || ''}</small>
            </td>
            <td><div class="flex items-center gap-2"><div class="avatar avatar--sm av-${emp?.avatar||1}">${(emp?.name||'?').split(' ')[0][0]}</div><small>${emp?.name.split(' ')[0]||''}</small></div></td>
            <td>${chipFor(t.status)}</td>
            <td><small class="text-muted">${fmtDateAr(t.due, calendar)}</small></td>
          </tr>`;
        }).join('')}
      </tbody>
    </table>`;
  }

  function chipFor(status) {
    const map = { new:'new', progress:'progress', done:'done', approved:'done', late:'late', paused:'paused', rejected:'rejected', archived:'archived' };
    return `<span class="chip chip--${map[status]}">${STATUS_LABELS[status]}</span>`;
  }

  // ============== TASKS (Kanban + List) ==============
  function viewTasks() {
    bcrumb.innerHTML = `<span>الإدارة</span> <span class="topbar__sep">/</span> <b>المهام</b>`;
    main.innerHTML = `
      <div class="page-head">
        <div>
          <h1>إدارة المهام</h1>
          <p>${state.tasks.length} مهمة · ${state.tasks.filter(t=>t.status==='progress').length} قيد التنفيذ</p>
        </div>
        <div class="page-head__actions">
          <button class="btn btn--ghost" onclick="window.toggleViewMode()">🔄 ${window.__viewMode==='list'?'عرض Kanban':'عرض جدولي'}</button>
          <button class="btn btn--primary" onclick="window.openNewTask()">+ مهمة جديدة</button>
        </div>
      </div>

      <div class="filters no-print">
        <div class="filters__group">
          <label class="text-sm text-muted">المشروع:</label>
          <select id="fProject"><option value="all">الكل</option>${state.projects.map(p=>`<option value="${p.id}">${p.name}</option>`).join('')}</select>
        </div>
        <div class="filters__group">
          <label class="text-sm text-muted">الموظف:</label>
          <select id="fEmp"><option value="all">الكل</option>${state.accounts.filter(a=>a.role!=='admin').map(e=>`<option value="${e.empId}">${e.name}</option>`).join('')}</select>
        </div>
        <div class="filters__group">
          <label class="text-sm text-muted">الأولوية:</label>
          <select id="fPriority"><option value="all">الكل</option><option value="high">عالية</option><option value="med">متوسطة</option><option value="low">منخفضة</option></select>
        </div>
        <div class="filters__group" style="margin-right:auto;">
          <input type="text" id="fSearch" placeholder="🔍 ابحث في عناوين المهام..." style="min-width: 240px;"/>
        </div>
      </div>

      <div id="tasksWrap"></div>
    `;
    document.querySelectorAll('.filters select, .filters input').forEach(el => el.addEventListener('input', renderTasks));
    renderTasks();
  }

  window.toggleViewMode = () => { window.__viewMode = window.__viewMode === 'list' ? 'kanban' : 'list'; viewTasks(); };

  function renderTasks() {
    const fProj = document.getElementById('fProject')?.value || 'all';
    const fEmp = document.getElementById('fEmp')?.value || 'all';
    const fPri = document.getElementById('fPriority')?.value || 'all';
    const fSearch = (document.getElementById('fSearch')?.value || '').toLowerCase();

    const list = state.tasks.filter(t =>
      (fProj === 'all' || t.projectId === fProj) &&
      (fEmp === 'all' || t.empId === fEmp) &&
      (fPri === 'all' || t.priority === fPri) &&
      (!fSearch || t.title.toLowerCase().includes(fSearch))
    );

    const wrap = document.getElementById('tasksWrap');
    if (window.__viewMode === 'list') {
      wrap.innerHTML = renderTaskTable(list);
    } else {
      wrap.innerHTML = renderKanban(list);
      bindKanbanDnD();
    }
  }

  function renderKanban(list) {
    const cols = [
      { id: 'new',      title: 'جديدة',     statuses: ['new'] },
      { id: 'progress', title: 'جاري العمل', statuses: ['progress','late'] },
      { id: 'done',     title: 'منتهية',    statuses: ['done'] },
      { id: 'approved', title: 'معتمدة',    statuses: ['approved'] }
    ];
    return `
      <div class="kanban">
        ${cols.map(c => {
          const tasks = list.filter(t => c.statuses.includes(t.status));
          return `
            <div class="kanban__col" data-status="${c.id}">
              <div class="kanban__col-head">
                <b>${c.title}</b>
                <span class="count">${tasks.length}</span>
              </div>
              <div class="kanban__list" data-target="${c.id}">
                ${tasks.map(t => kanbanCard(t)).join('')}
              </div>
            </div>`;
        }).join('')}
      </div>`;
  }

  function kanbanCard(t) {
    const emp = state.accounts.find(a => a.empId === t.empId);
    const proj = state.projects.find(p => p.id === t.projectId);
    const late = isLate(t);
    return `
      <div class="task-card" draggable="true" data-id="${t.id}" onclick="openTask('${t.id}')">
        <div class="task-card__head">
          <span class="task-card__project">${proj?.name.slice(0,18) || '—'}</span>
          <span class="priority-dot priority-dot--${t.priority==='high'?'high':t.priority==='med'?'med':'low'}" title="${PRIORITY_LABELS[t.priority]}"></span>
        </div>
        <div class="task-card__title">${t.title}</div>
        <div class="task-card__meta">
          <div class="flex items-center gap-2">
            <div class="avatar avatar--sm av-${emp?.avatar||1}">${(emp?.name||'؟')[0]}</div>
            <small>${emp?.name.split(' ')[0]||''}</small>
          </div>
          <span class="task-card__date ${late?'late':''}">${late?'⚠':'📅'} ${fmtDateAr(t.due, calendar)}</span>
        </div>
      </div>`;
  }

  function bindKanbanDnD() {
    let dragId = null;
    document.querySelectorAll('.task-card').forEach(card => {
      card.addEventListener('dragstart', e => { dragId = card.dataset.id; card.classList.add('dragging'); e.dataTransfer.effectAllowed = 'move'; });
      card.addEventListener('dragend', () => { card.classList.remove('dragging'); document.querySelectorAll('.kanban__list').forEach(c => c.classList.remove('drag-over')); });
    });
    document.querySelectorAll('.kanban__list').forEach(list => {
      list.addEventListener('dragover', e => { e.preventDefault(); list.classList.add('drag-over'); });
      list.addEventListener('dragleave', () => list.classList.remove('drag-over'));
      list.addEventListener('drop', e => {
        e.preventDefault();
        list.classList.remove('drag-over');
        const target = list.dataset.target;
        if (!dragId) return;
        const task = state.tasks.find(t => t.id === dragId);
        if (!task) return;
        task.status = target === 'progress' ? 'progress' : target;
        Store.save(state);
        toast('success', 'تم نقل المهمة', `الحالة الجديدة: ${STATUS_LABELS[task.status]}`);
        renderTasks();
      });
    });
  }

  function renderTaskTable(list) {
    if (!list.length) return `<div class="card"><div class="empty"><div class="empty__icon">📭</div><b>لا توجد مهام مطابقة</b><p>جرّب تعديل الفلاتر</p></div></div>`;
    return `<div class="card"><table class="table table--clickable">
      <thead>
        <tr>
          <th>المهمة</th><th>المشروع</th><th>الموظف</th><th>الحالة</th><th>الأولوية</th><th>التسليم</th><th>الوقت</th>
        </tr>
      </thead>
      <tbody>
        ${list.map(t => {
          const emp = state.accounts.find(a => a.empId === t.empId);
          const proj = state.projects.find(p => p.id === t.projectId);
          return `<tr onclick="openTask('${t.id}')">
            <td><div class="table__title">${t.title}</div></td>
            <td><small>${proj?.name||'—'}</small></td>
            <td><div class="flex items-center gap-2"><div class="avatar avatar--sm av-${emp?.avatar||1}">${(emp?.name||'?')[0]}</div><small>${emp?.name.split(' ')[0]||''}</small></div></td>
            <td>${chipFor(t.status)}</td>
            <td><span class="priority-dot priority-dot--${t.priority==='high'?'high':t.priority==='med'?'med':'low'}"></span> ${PRIORITY_LABELS[t.priority]}</td>
            <td><small class="${isLate(t)?'text-danger font-bold':''}">${fmtDateAr(t.due, calendar)}</small></td>
            <td><small>${t.timeSpent}</small></td>
          </tr>`;
        }).join('')}
      </tbody>
    </table></div>`;
  }

  // ============== TASK DETAIL DRAWER ==============
  window.openTask = function(id) {
    const t = state.tasks.find(x => x.id === id);
    if (!t) return;
    const emp = state.accounts.find(a => a.empId === t.empId);
    const proj = state.projects.find(p => p.id === t.projectId);
    const comments = state.comments.filter(c => c.taskId === id);

    document.getElementById('drawerTitle').textContent = t.title;
    document.getElementById('drawerBody').innerHTML = `
      <div class="flex items-center gap-2 mb-3">
        ${chipFor(t.status)}
        <span class="chip" style="background: var(--bg); color: var(--text-muted);">
          <span class="priority-dot priority-dot--${t.priority==='high'?'high':t.priority==='med'?'med':'low'}"></span> ${PRIORITY_LABELS[t.priority]}
        </span>
      </div>

      <div class="detail-meta">
        <div class="detail-meta__item"><small>المشروع</small><span>${proj?.name||'—'}</span></div>
        <div class="detail-meta__item"><small>الموظف المسؤول</small><span class="flex items-center gap-2"><div class="avatar avatar--sm av-${emp?.avatar||1}">${(emp?.name||'?')[0]}</div>${emp?.name||'—'}</span></div>
        <div class="detail-meta__item"><small>تاريخ البداية</small><span>${fmtDateAr(t.start, calendar)}</span></div>
        <div class="detail-meta__item"><small>تاريخ التسليم</small><span class="${isLate(t)?'text-danger':''}">${fmtDateAr(t.due, calendar)}</span></div>
        <div class="detail-meta__item"><small>الوقت المستغرق</small><span>${t.timeSpent}</span></div>
        <div class="detail-meta__item"><small>المرفقات</small><span>${t.attachments} ملفات</span></div>
      </div>

      <div class="section">
        <h4>الوصف</h4>
        <p style="font-size: 13px; line-height: 1.7; color: var(--text-muted);">
          هذه المهمة جزء من مشروع <b>${proj?.name||'—'}</b>. يجب الالتزام بالموعد المحدد ومراجعة جميع الاشتراطات قبل الإنهاء.
          الرجاء إضافة التعليقات اللازمة وإرفاق الملفات المطلوبة.
        </p>
      </div>

      <div class="section">
        <h4>المرفقات (${t.attachments})</h4>
        <div class="attach-list">
          ${t.attachments > 0 ? `
            <div class="attach-item">
              <div class="attach-item__icon">PDF</div>
              <div class="attach-item__name">المتطلبات_التقنية.pdf</div>
              <div class="attach-item__size">1.2 MB</div>
              <button class="btn btn--sm btn--soft">⬇</button>
            </div>
            ${t.attachments > 1 ? `<div class="attach-item">
              <div class="attach-item__icon">DOC</div>
              <div class="attach-item__name">مواصفات_العميل.docx</div>
              <div class="attach-item__size">450 KB</div>
              <button class="btn btn--sm btn--soft">⬇</button>
            </div>` : ''}
          ` : '<small class="text-muted">لا توجد مرفقات</small>'}
        </div>
        <button class="btn btn--ghost btn--sm mt-2" onclick="toast('success','تم رفع الملف','test-document.pdf — 1.4 MB')">+ ارفع ملف</button>
      </div>

      <div class="section">
        <h4>التعليقات (${comments.length})</h4>
        ${comments.length ? comments.map(c => {
          const a = state.accounts.find(x => x.empId === c.empId);
          return `<div class="comment">
            <div class="avatar avatar--sm av-${a?.avatar||1}">${(a?.name||'?')[0]}</div>
            <div class="comment__body">
              <div class="comment__head"><b>${a?.name||'—'}</b><small>${c.date}</small></div>
              <div class="comment__text">${c.text}</div>
            </div>
          </div>`;
        }).join('') : '<small class="text-muted">لا توجد تعليقات بعد</small>'}
        <div class="comment-input">
          <textarea id="newComment" placeholder="اكتب تعليقاً..."></textarea>
          <button class="btn btn--primary" onclick="window.addComment('${id}')">إضافة</button>
        </div>
      </div>
    `;

    const isMine = t.empId === session.empId;
    const isAdmin = session.role === 'admin';
    let actions = '';
    if (isMine && t.status === 'new') {
      actions = `<button class="btn btn--primary" onclick="window.changeTaskStatus('${id}','progress','تم قبول المهمة')">✓ قبول المهمة</button>
                 <button class="btn btn--ghost" onclick="window.changeTaskStatus('${id}','rejected','تم رفض المهمة')">✗ رفض</button>`;
    } else if (isMine && (t.status === 'progress' || t.status === 'late')) {
      actions = `<button class="btn btn--primary" onclick="window.changeTaskStatus('${id}','done','تم إنهاء المهمة')">🏁 إنهاء المهمة</button>`;
    } else if (isAdmin && t.status === 'done') {
      actions = `<button class="btn btn--primary" onclick="window.changeTaskStatus('${id}','approved','تم اعتماد الإنجاز')">✓ اعتماد</button>
                 <button class="btn btn--ghost" onclick="window.changeTaskStatus('${id}','progress','تم رفض الإنجاز — أعيدت للموظف')">✗ رفض الإنجاز</button>`;
    } else if (isAdmin) {
      actions = `<button class="btn btn--ghost">✏ تعديل</button>
                 <button class="btn btn--ghost">↻ إعادة إسناد</button>`;
    }
    document.getElementById('drawerFoot').innerHTML = actions;

    document.getElementById('drawer').classList.add('show');
    document.getElementById('drawerBackdrop').classList.add('show');
  };

  window.addComment = function (id) {
    const text = document.getElementById('newComment').value.trim();
    if (!text) return;
    state.comments.push({ taskId: id, empId: session.empId, text, date: new Date().toISOString().slice(0,16).replace('T',' ') });
    Store.save(state);
    openTask(id);
    toast('success', 'تم إضافة التعليق', '');
  };

  window.changeTaskStatus = function (id, newStatus, msg) {
    const t = state.tasks.find(x => x.id === id);
    if (!t) return;
    t.status = newStatus;
    Store.save(state);
    toast('success', msg, `الحالة الجديدة: ${STATUS_LABELS[newStatus]}`);
    closeDrawer();
    if (window.__currentRoute === 'tasks') renderTasks();
    else navigate(location.hash.slice(1) || (session.role==='admin'?'dashboard':'emp-dashboard'));
  };

  function closeDrawer() {
    document.getElementById('drawer').classList.remove('show');
    document.getElementById('drawerBackdrop').classList.remove('show');
  }
  document.getElementById('drawerClose').addEventListener('click', closeDrawer);
  document.getElementById('drawerBackdrop').addEventListener('click', closeDrawer);

  window.openNewTask = function () {
    const m = document.getElementById('modalBackdrop');
    document.getElementById('modalTitle').textContent = 'إنشاء مهمة جديدة';
    document.getElementById('modalBody').innerHTML = `
      <div class="field"><label>عنوان المهمة <span>*</span></label><input id="ntTitle" type="text" placeholder="مثال: مراجعة تصميم الواجهة"/></div>
      <div class="field"><label>المشروع <span>*</span></label><select id="ntProj">${state.projects.map(p=>`<option value="${p.id}">${p.name}</option>`).join('')}</select></div>
      <div class="field"><label>الموظف المسؤول <span>*</span></label><select id="ntEmp">${state.accounts.filter(a=>a.role!=='admin'&&a.active).map(e=>`<option value="${e.empId}">${e.name}</option>`).join('')}</select></div>
      <div class="row row--2" style="grid-template-columns: 1fr 1fr;">
        <div class="field"><label>تاريخ البداية</label><input id="ntStart" type="date" value="2026-05-04"/></div>
        <div class="field"><label>تاريخ التسليم</label><input id="ntDue" type="date" value="2026-05-15"/></div>
      </div>
      <div class="field"><label>الأولوية</label><select id="ntPri"><option value="high">عالية</option><option value="med" selected>متوسطة</option><option value="low">منخفضة</option></select></div>
    `;
    document.getElementById('modalFoot').innerHTML = `
      <button class="btn btn--primary" onclick="window.saveTask()">حفظ المهمة</button>
      <button class="btn btn--ghost" onclick="document.getElementById('modalBackdrop').classList.remove('show')">إلغاء</button>
    `;
    m.classList.add('show');
  };

  window.saveTask = function () {
    const title = document.getElementById('ntTitle').value.trim();
    if (!title) return toast('danger', 'العنوان مطلوب', '');
    const newId = 'T' + (state.tasks.length + 1).toString().padStart(2,'0');
    state.tasks.unshift({
      id: newId,
      title,
      projectId: document.getElementById('ntProj').value,
      empId: document.getElementById('ntEmp').value,
      status: 'new',
      priority: document.getElementById('ntPri').value,
      start: document.getElementById('ntStart').value,
      due: document.getElementById('ntDue').value,
      timeSpent: '0:00', comments: 0, attachments: 0
    });
    Store.save(state);
    document.getElementById('modalBackdrop').classList.remove('show');
    toast('success', 'تم إنشاء المهمة بنجاح', title);
    renderTasks();
  };

  // ============== PROJECTS ==============
  function viewProjects() {
    bcrumb.innerHTML = `<span>الإدارة</span> <span class="topbar__sep">/</span> <b>المشاريع</b>`;
    main.innerHTML = `
      <div class="page-head">
        <div><h1>المشاريع</h1><p>${state.projects.length} مشاريع نشطة</p></div>
        <div class="page-head__actions"><button class="btn btn--primary">+ مشروع جديد</button></div>
      </div>
      <div class="proj-grid">
        ${state.projects.map(p => {
          const tasks = state.tasks.filter(t => t.projectId === p.id);
          const done = tasks.filter(t => t.status === 'done' || t.status === 'approved').length;
          const pct = tasks.length ? Math.round(done / tasks.length * 100) : 0;
          const empSet = new Set(tasks.map(t => t.empId));
          return `
            <div class="proj-card" onclick="navigate('project-detail','${p.id}')">
              <div class="proj-card__head">
                <div class="proj-card__title">${p.name}</div>
                <span class="chip ${p.status==='done'?'chip--done':p.status==='active'?'chip--progress':'chip--new'}">${p.status==='done'?'مكتمل':p.status==='active'?'نشط':'تخطيط'}</span>
              </div>
              <div class="proj-card__desc">${p.desc}</div>
              <div class="proj-card__progress"><div class="proj-card__progress-bar" style="width: ${pct}%; background: ${p.color}"></div></div>
              <div class="flex justify-between mb-2"><small class="text-muted">التقدم</small><b style="font-size: 12px;">${pct}%</b></div>
              <div class="proj-card__stats">
                <div><b>${tasks.length}</b>المهام</div>
                <div><b>${done}</b>منجزة</div>
                <div><b>${empSet.size}</b>الفريق</div>
              </div>
            </div>`;
        }).join('')}
      </div>
    `;
  }

  function viewProjectDetail(id) {
    const p = state.projects.find(x => x.id === id) || state.projects[0];
    bcrumb.innerHTML = `<span>المشاريع</span> <span class="topbar__sep">/</span> <b>${p.name}</b>`;
    const tasks = state.tasks.filter(t => t.projectId === p.id);
    const done = tasks.filter(t => t.status === 'done' || t.status === 'approved').length;
    const pct = tasks.length ? Math.round(done / tasks.length * 100) : 0;

    main.innerHTML = `
      <div class="page-head">
        <div>
          <h1 style="display: flex; align-items: center; gap: 12px;">
            <span style="width: 14px; height: 36px; background: ${p.color}; border-radius: 3px;"></span>
            ${p.name}
          </h1>
          <p>${p.desc}</p>
        </div>
        <div class="page-head__actions">
          <button class="btn btn--ghost" onclick="navigate('projects')">← العودة</button>
          <button class="btn btn--primary" onclick="window.openNewTask()">+ مهمة</button>
        </div>
      </div>

      <div class="kpi-grid">
        ${kpiCard('إجمالي المهام', tasks.length, 'blue', 'neutral', '·', '📋', [3,4,5,6,7,8,9,10])}
        ${kpiCard('منجزة', done, 'green', 'up', `${pct}%`, '✓', [1,2,3,4,5,6,7,done])}
        ${kpiCard('قيد التنفيذ', tasks.filter(t=>t.status==='progress').length, 'amber', 'neutral', '·', '⏱', [1,2,2,3,3,4,4,5])}
        ${kpiCard('متأخرة', tasks.filter(t=>t.status==='late').length, 'red', 'down', '-1', '⚠', [2,2,1,1,1,1,0,1])}
      </div>

      <div class="card">
        <div class="card__head"><h3>مهام المشروع</h3><small>${tasks.length} مهمة</small></div>
        ${renderTaskTable(tasks)}
      </div>
    `;
  }

  // ============== EMPLOYEES ==============
  function viewEmployees() {
    bcrumb.innerHTML = `<span>الإدارة</span> <span class="topbar__sep">/</span> <b>الموظفين</b>`;
    const emps = state.accounts.filter(a => a.role !== 'admin');
    main.innerHTML = `
      <div class="page-head">
        <div><h1>إدارة الموظفين</h1><p>${emps.filter(e=>e.active).length} موظف نشط · ${emps.filter(e=>!e.active).length} موقوف</p></div>
        <div class="page-head__actions"><button class="btn btn--primary" onclick="window.openNewEmp()">+ إضافة موظف</button></div>
      </div>

      <div class="card">
        <table class="table">
          <thead><tr><th>الموظف</th><th>المسمى الوظيفي</th><th>البريد الإلكتروني</th><th>الجوال</th><th>المهام</th><th>الحالة</th><th></th></tr></thead>
          <tbody>
            ${emps.map(e => {
              const myTasks = state.tasks.filter(t => t.empId === e.empId);
              const myDone = myTasks.filter(t => t.status === 'done' || t.status === 'approved').length;
              return `<tr>
                <td><div class="flex items-center gap-3"><div class="avatar av-${e.avatar}">${e.name.split(' ').map(s=>s[0]).slice(0,2).join('')}</div><div><b>${e.name}</b><small style="display:block; color:var(--text-muted);">${e.username}</small></div></div></td>
                <td><small>${e.title}</small></td>
                <td><small>${e.email}</small></td>
                <td><small dir="ltr">${e.phone}</small></td>
                <td><small><b>${myTasks.length}</b> مهمة · <span class="text-success">${myDone} منجزة</span></small></td>
                <td>${e.active ? '<span class="chip chip--done">نشط</span>' : '<span class="chip chip--late">موقوف</span>'}</td>
                <td><button class="btn btn--sm btn--ghost" onclick="window.toggleEmp('${e.empId}')">${e.active?'إيقاف':'تفعيل'}</button></td>
              </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>
    `;
  }

  window.toggleEmp = function(id) {
    const e = state.accounts.find(a => a.empId === id);
    e.active = !e.active;
    Store.save(state);
    toast(e.active?'success':'warning', e.active?'تم تفعيل الموظف':'تم إيقاف الموظف', e.name);
    viewEmployees();
  };

  window.openNewEmp = function () {
    document.getElementById('modalTitle').textContent = 'إضافة موظف جديد';
    document.getElementById('modalBody').innerHTML = `
      <div class="field"><label>الاسم الكامل *</label><input id="neName" type="text" placeholder="الاسم الثلاثي"/></div>
      <div class="field"><label>اسم المستخدم *</label><input id="neUser" type="text" placeholder="username"/></div>
      <div class="field"><label>البريد الإلكتروني *</label><input id="neEmail" type="email" placeholder="email@esnad.sa"/></div>
      <div class="field"><label>الجوال *</label><input id="nePhone" type="tel" placeholder="05xxxxxxxx"/></div>
      <div class="field"><label>المسمى الوظيفي</label><input id="neTitle" type="text" placeholder="مطور / مصمم / محلل ..."/></div>
    `;
    document.getElementById('modalFoot').innerHTML = `
      <button class="btn btn--primary" onclick="window.saveEmp()">حفظ</button>
      <button class="btn btn--ghost" onclick="document.getElementById('modalBackdrop').classList.remove('show')">إلغاء</button>
    `;
    document.getElementById('modalBackdrop').classList.add('show');
  };

  window.saveEmp = function () {
    const name = document.getElementById('neName').value.trim();
    const username = document.getElementById('neUser').value.trim();
    if (!name || !username) return toast('danger', 'بيانات ناقصة', 'الاسم واسم المستخدم مطلوبان');
    const id = 'E' + String(state.accounts.length + 1).padStart(2, '0');
    state.accounts.push({
      domain: 'esnad', username, password: '123456', role: 'employee',
      empId: id, name, title: document.getElementById('neTitle').value || 'موظف',
      email: document.getElementById('neEmail').value, phone: document.getElementById('nePhone').value,
      avatar: (state.accounts.length % 6) + 1, active: true
    });
    Store.save(state);
    document.getElementById('modalBackdrop').classList.remove('show');
    toast('success', 'تم إضافة الموظف', name);
    viewEmployees();
  };

  // ============== REPORTS ==============
  function viewReports() {
    bcrumb.innerHTML = `<span>التحليلات</span> <span class="topbar__sep">/</span> <b>التقارير</b>`;
    const t = state.tasks;
    const total = t.length, done = t.filter(x=>x.status==='done'||x.status==='approved').length;
    main.innerHTML = `
      <div class="page-head no-print">
        <div><h1>التقارير</h1><p>تقرير شامل لأداء الفريق والمشاريع</p></div>
        <div class="page-head__actions">
          <select id="reportPeriod" style="height:38px; padding: 0 12px; border: 1px solid var(--border); border-radius: var(--radius);">
            <option>هذا الشهر</option><option>الربع الحالي</option><option>السنة الحالية</option>
          </select>
          <button class="btn btn--ghost" onclick="window.print()">🖨 طباعة / PDF</button>
        </div>
      </div>

      <div class="report-page">
        <div class="report-page__head">
          <div style="font-size: 28px; font-weight: 800; color: var(--primary);">إسناد</div>
          <h2>تقرير الأداء الشامل</h2>
          <p>${state.company.name} · من ٢٠٢٦/٠٤/٠١ إلى ٢٠٢٦/٠٤/٣٠</p>
        </div>

        <div class="report-meta-row">
          <div><b>تاريخ التقرير:</b> ${fmtDateAr('2026-05-04', calendar)}</div>
          <div><b>المُعد:</b> ${session.name}</div>
          <div><b>التقويم:</b> ${calendar==='hijri'?'هجري':'ميلادي'}</div>
        </div>

        <div class="kpi-grid">
          ${kpiCard('إجمالي المهام', total, 'blue', 'up', '+8%', '📋', [12,14,13,16,18,17,20,22])}
          ${kpiCard('معدل الإنجاز', Math.round(done/total*100)+'%', 'green', 'up', '+12%', '✓', [40,45,50,55,60,62,68,72])}
          ${kpiCard('متوسط زمن الإنجاز', '4.2 يوم', 'amber', 'down', '-0.5', '⏱', [5,5,4.5,4.5,4.3,4.2,4.2,4.2])}
          ${kpiCard('عدد الموظفين', state.accounts.filter(a=>a.role==='employee'&&a.active).length, 'blue', 'neutral', '·', '👥', [4,4,4,4,4,4,4,4])}
        </div>

        <div class="row row--2 mb-4">
          <div class="card">
            <div class="card__head"><h3>توزيع المهام</h3></div>
            <div class="card__body" id="rDonut"></div>
          </div>
          <div class="card">
            <div class="card__head"><h3>أعلى المنفذين</h3></div>
            <div class="card__body" id="rBar"></div>
          </div>
        </div>

        <div class="card mb-4">
          <div class="card__head"><h3>الإنجاز الشهري</h3></div>
          <div class="card__body">${Charts.vbar([
            {label:'ديسمبر',value:18},{label:'يناير',value:22},{label:'فبراير',value:27},{label:'مارس',value:31},{label:'أبريل',value:28},{label:'مايو',value:done}
          ], { h: 220 })}</div>
        </div>

        <div class="card">
          <div class="card__head"><h3>تفاصيل المهام في الفترة</h3></div>
          ${renderTaskTable(t.slice(0, 10))}
        </div>
      </div>
    `;

    const counts = ['new','progress','done','approved','late'].map(s => ({
      label: STATUS_LABELS[s], value: t.filter(x=>x.status===s).length,
      color: ({new:'#3d85c6', progress:'#6366f1', done:'#10b981', approved:'#8b5cf6', late:'#ef4444'})[s]
    })).filter(d => d.value > 0);
    document.getElementById('rDonut').innerHTML = Charts.donut(counts, { size: 220, label: 'مهمة' });

    const perEmp = state.accounts.filter(a => a.role === 'employee' && a.active).map(a => ({
      label: a.name.split(' ').slice(0,2).join(' '),
      value: t.filter(x => x.empId === a.empId).length,
      color: ({1:'#3d85c6',2:'#10b981',3:'#f59e0b',4:'#8b5cf6',5:'#ef4444',6:'#06b6d4'})[a.avatar]
    })).sort((a,b)=>b.value-a.value);
    const bar = document.getElementById('rBar');
    bar.innerHTML = Charts.bar(perEmp);
    Charts.animateBars(bar);
  }

  // ============== ATTENDANCE ==============
  function viewAttendance() {
    bcrumb.innerHTML = `<span>التحليلات</span> <span class="topbar__sep">/</span> <b>الحضور والانصراف</b>`;
    const data = state.attendance;
    main.innerHTML = `
      <div class="page-head">
        <div><h1>الحضور والانصراف</h1><p>سجل حضور وانصراف الموظفين</p></div>
        <div class="page-head__actions">
          <select style="height:38px; padding:0 12px; border:1px solid var(--border); border-radius: var(--radius);">
            ${state.accounts.filter(a=>a.role==='employee').map(e=>`<option>${e.name}</option>`).join('')}
          </select>
          <button class="btn btn--ghost" onclick="window.print()">🖨 طباعة</button>
        </div>
      </div>

      <div class="card">
        <table class="table">
          <thead><tr><th>التاريخ</th><th>اليوم</th><th>الحضور</th><th>الانصراف</th><th>عدد الساعات</th><th>الحالة</th></tr></thead>
          <tbody>
            ${data.map(a => {
              const day = new Date(a.date).toLocaleDateString('ar-EG', { weekday: 'long' });
              return `<tr>
                <td>${fmtDateAr(a.date, calendar)}</td>
                <td>${day}</td>
                <td><b dir="ltr" style="color: var(--success);">${a.in}</b></td>
                <td><b dir="ltr">${a.out || '<span class="text-muted">لم يُسجَّل</span>'}</b></td>
                <td><b>${a.hours}</b></td>
                <td>${a.out ? '<span class="chip chip--done">مكتمل</span>' : '<span class="chip chip--progress">نشط الآن</span>'}</td>
              </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>
    `;
  }

  // ============== ACTIVITY LOG ==============
  function viewActivity() {
    bcrumb.innerHTML = `<span>التحليلات</span> <span class="topbar__sep">/</span> <b>سجل العمليات</b>`;
    main.innerHTML = `
      <div class="page-head">
        <div><h1>سجل العمليات</h1><p>تاريخ كامل لكل ما يحدث في النظام</p></div>
      </div>
      <div class="card">
        <div class="card__body">${activityList(state.activities)}</div>
      </div>
    `;
  }

  // ============== PROFILE ==============
  function viewProfile() {
    bcrumb.innerHTML = `<span>الحساب</span> <span class="topbar__sep">/</span> <b>الملف الشخصي</b>`;
    const u = session;
    main.innerHTML = `
      <div class="page-head"><h1>الملف الشخصي</h1></div>
      <div class="profile-card mb-4">
        <div class="avatar avatar--lg av-${u.avatar}">${u.name.split(' ').map(s=>s[0]).slice(0,2).join('')}</div>
        <div class="profile-card__info">
          <div class="profile-card__name">${u.name}</div>
          <div class="profile-card__role">${u.title} · ${state.company.name}</div>
          <div class="profile-card__meta">
            <div class="profile-card__meta-item"><small>اسم المستخدم</small><b>${u.username}</b></div>
            <div class="profile-card__meta-item"><small>البريد الإلكتروني</small><b>${u.email}</b></div>
            <div class="profile-card__meta-item"><small>رقم الجوال</small><b dir="ltr">${u.phone}</b></div>
            <div class="profile-card__meta-item"><small>الدومين</small><b dir="ltr">${u.domain}.telesak.app</b></div>
          </div>
        </div>
        <div><button class="btn btn--ghost">✏ تعديل</button></div>
      </div>

      <div class="card">
        <div class="card__head"><h3>تغيير كلمة المرور</h3></div>
        <div class="card__body">
          <div class="row" style="grid-template-columns: 1fr 1fr 1fr; gap: 14px;">
            <div class="field"><label>كلمة المرور الحالية</label><input type="password"/></div>
            <div class="field"><label>كلمة المرور الجديدة</label><input type="password"/></div>
            <div class="field"><label>تأكيد كلمة المرور</label><input type="password"/></div>
          </div>
          <button class="btn btn--primary mt-3" onclick="toast('success','تم تحديث كلمة المرور','')">حفظ التغيير</button>
        </div>
      </div>
    `;
  }

  // ============== HELP ==============
  function viewHelp() {
    bcrumb.innerHTML = `<b>المساعدة</b>`;
    main.innerHTML = `
      <div class="page-head"><div><h1>مركز المساعدة</h1><p>أدلة الاستخدام والدعم الفني</p></div></div>
      <div class="help-grid">
        <div class="help-card">
          <div class="help-card__icon">📘</div>
          <b>دليل المستخدم</b>
          <small>الدليل الكامل لاستخدام المنصة</small>
        </div>
        <div class="help-card">
          <div class="help-card__icon">🎓</div>
          <b>ابدأ الآن</b>
          <small>دليل سريع للموظفين الجدد</small>
        </div>
        <div class="help-card">
          <div class="help-card__icon">❓</div>
          <b>الأسئلة الشائعة</b>
          <small>إجابات على أكثر الأسئلة شيوعاً</small>
        </div>
        <div class="help-card">
          <div class="help-card__icon">💬</div>
          <b>الدعم الفني</b>
          <small>تواصل معنا عبر واتساب</small>
        </div>
        <div class="help-card">
          <div class="help-card__icon">🎥</div>
          <b>فيديوهات تعليمية</b>
          <small>شروحات مرئية لجميع الميزات</small>
        </div>
        <div class="help-card">
          <div class="help-card__icon">📧</div>
          <b>راسلنا</b>
          <small>support@telesak.app</small>
        </div>
      </div>
    `;
  }

  // ============== EMPLOYEE VIEWS ==============
  function viewEmpDashboard() {
    bcrumb.innerHTML = `<b>الرئيسية</b>`;
    const my = state.tasks.filter(t => t.empId === session.empId);
    const today = my.filter(t => t.due === '2026-05-04').length;
    const newCount = my.filter(t => t.status === 'new').length;
    const progress = my.filter(t => t.status === 'progress').length;
    const late = my.filter(t => t.status === 'late').length;
    const todayAtt = state.attendance.find(a => a.empId === session.empId && a.date === '2026-05-04');

    main.innerHTML = `
      <div class="page-head">
        <div><h1>أهلاً ${session.name.split(' ')[0]} 👋</h1><p>إليك ملخص يومك</p></div>
        <div class="page-head__actions">
          <div class="card" style="padding: 8px 14px; display: flex; align-items: center; gap: 10px; box-shadow: var(--shadow-sm);">
            <span class="chip chip--done">🟢 حضور نشط</span>
            <small>منذ <b dir="ltr">${todayAtt?.in || '08:32'}</b></small>
          </div>
        </div>
      </div>

      <div class="kpi-grid">
        ${kpiCard('مهام جديدة', newCount, 'blue', 'neutral', '·', '🆕', [1,2,1,2,3,2,3,newCount])}
        ${kpiCard('قيد التنفيذ', progress, 'amber', 'up', '+1', '⏱', [1,1,2,2,2,3,3,progress])}
        ${kpiCard('متأخرة', late, 'red', 'down', '-1', '⚠', [2,2,1,1,1,1,0,late])}
        ${kpiCard('تسليمات اليوم', today, 'green', 'neutral', '·', '🎯', [0,1,2,1,2,1,2,today])}
      </div>

      <div class="row row--2 mb-4">
        <div class="card">
          <div class="card__head"><h3>مهامي اليوم وغداً</h3></div>
          <div class="card__body" style="padding: 0;">
            ${my.filter(t => t.status !== 'done' && t.status !== 'approved').slice(0, 5).map(t => {
              const proj = state.projects.find(p => p.id === t.projectId);
              return `<div onclick="openTask('${t.id}')" style="padding: 14px 20px; border-bottom: 1px solid var(--border); cursor: pointer; display: flex; align-items: center; gap: 12px;">
                <span class="priority-dot priority-dot--${t.priority==='high'?'high':t.priority==='med'?'med':'low'}"></span>
                <div style="flex:1;">
                  <b style="display:block;">${t.title}</b>
                  <small class="text-muted">${proj?.name||''} · ${fmtDateAr(t.due, calendar)}</small>
                </div>
                ${chipFor(t.status)}
              </div>`;
            }).join('') || '<div class="empty"><div class="empty__icon">🎉</div><b>لا توجد مهام معلقة!</b></div>'}
          </div>
        </div>

        <div class="card">
          <div class="card__head"><h3>إنجازي هذا الأسبوع</h3></div>
          <div class="card__body">
            <div style="text-align: center; margin-bottom: 14px;">
              <div style="font-size: 48px; font-weight: 800; color: var(--success); line-height: 1;">${my.filter(t=>t.status==='done'||t.status==='approved').length}</div>
              <small class="text-muted">مهمة منجزة</small>
            </div>
            ${Charts.spark([2,3,4,3,5,6,7], { w: 400, h: 80, color: '#10b981' })}
            <div class="flex justify-between mt-3"><small class="text-muted">السبت</small><small class="text-muted">الجمعة</small></div>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card__head"><h3>آخر الإشعارات</h3></div>
        <div class="card__body">${activityList(state.activities.slice(0,5))}</div>
      </div>
    `;
  }

  function viewEmpTasks() {
    bcrumb.innerHTML = `<b>مهامي</b>`;
    const my = state.tasks.filter(t => t.empId === session.empId);
    main.innerHTML = `
      <div class="page-head">
        <div><h1>مهامي</h1><p>${my.length} مهمة موكلة إليك</p></div>
      </div>
      <div class="tabs">
        <button class="active" data-tab="all">الكل <span class="tab-count">${my.length}</span></button>
        <button data-tab="new">جديدة <span class="tab-count">${my.filter(t=>t.status==='new').length}</span></button>
        <button data-tab="progress">جاري <span class="tab-count">${my.filter(t=>t.status==='progress'||t.status==='late').length}</span></button>
        <button data-tab="done">منتهية <span class="tab-count">${my.filter(t=>t.status==='done'||t.status==='approved').length}</span></button>
      </div>
      <div id="empTaskWrap"></div>
    `;
    const wrap = document.getElementById('empTaskWrap');
    const render = (tab) => {
      let list = my;
      if (tab === 'new') list = my.filter(t => t.status === 'new');
      else if (tab === 'progress') list = my.filter(t => t.status === 'progress' || t.status === 'late');
      else if (tab === 'done') list = my.filter(t => t.status === 'done' || t.status === 'approved');
      wrap.innerHTML = renderTaskTable(list);
    };
    render('all');
    document.querySelectorAll('.tabs button').forEach(b => b.addEventListener('click', () => {
      document.querySelectorAll('.tabs button').forEach(x => x.classList.remove('active'));
      b.classList.add('active');
      render(b.dataset.tab);
    }));
  }

  function viewEmpReports() {
    bcrumb.innerHTML = `<b>تقاريري</b>`;
    const my = state.tasks.filter(t => t.empId === session.empId);
    const done = my.filter(t => t.status === 'done' || t.status === 'approved').length;
    main.innerHTML = `
      <div class="page-head">
        <div><h1>تقاريري</h1><p>إحصائيات أدائي الشخصي</p></div>
        <div class="page-head__actions"><button class="btn btn--ghost" onclick="window.print()">🖨 طباعة</button></div>
      </div>
      <div class="kpi-grid">
        ${kpiCard('إجمالي مهامي', my.length, 'blue', 'up', '+3', '📋', [3,4,5,6,7,8,9,my.length])}
        ${kpiCard('معدل الإنجاز', Math.round(done/(my.length||1)*100)+'%', 'green', 'up', '+10%', '✓', [40,50,55,60,65,68,70,72])}
        ${kpiCard('متوسط الوقت', '3.8 يوم', 'amber', 'down', '-0.4', '⏱', [5,5,4.5,4.2,4,4,3.9,3.8])}
        ${kpiCard('متأخرة', my.filter(t=>t.status==='late').length, 'red', 'neutral', '·', '⚠', [1,1,1,1,1,1,1,1])}
      </div>
      <div class="card mb-4">
        <div class="card__head"><h3>توزيع مهامي</h3></div>
        <div class="card__body" id="empDonut"></div>
      </div>
      <div class="card">
        <div class="card__head"><h3>تفاصيل المهام</h3></div>
        ${renderTaskTable(my)}
      </div>
    `;
    const counts = ['new','progress','done','approved','late'].map(s => ({
      label: STATUS_LABELS[s], value: my.filter(x=>x.status===s).length,
      color: ({new:'#3d85c6',progress:'#6366f1',done:'#10b981',approved:'#8b5cf6',late:'#ef4444'})[s]
    })).filter(d => d.value > 0);
    document.getElementById('empDonut').innerHTML = Charts.donut(counts, { size: 220, label: 'مهمة' });
  }

  function viewEmpAttendance() {
    bcrumb.innerHTML = `<b>حضوري</b>`;
    const my = state.attendance.filter(a => a.empId === session.empId);
    main.innerHTML = `
      <div class="page-head">
        <div><h1>سجل حضوري</h1><p>آخر ${my.length} أيام</p></div>
        <div class="page-head__actions"><button class="btn btn--ghost" onclick="window.print()">🖨 طباعة</button></div>
      </div>
      <div class="card">
        <table class="table">
          <thead><tr><th>التاريخ</th><th>اليوم</th><th>الحضور</th><th>الانصراف</th><th>عدد الساعات</th></tr></thead>
          <tbody>
            ${my.map(a => `<tr>
              <td>${fmtDateAr(a.date, calendar)}</td>
              <td>${new Date(a.date).toLocaleDateString('ar-EG', { weekday: 'long' })}</td>
              <td><b dir="ltr" style="color: var(--success);">${a.in}</b></td>
              <td><b dir="ltr">${a.out || '<span class="text-muted">—</span>'}</b></td>
              <td><b>${a.hours}</b></td>
            </tr>`).join('')}
          </tbody>
        </table>
      </div>
    `;
  }

  // ============== SHELL EVENTS ==============
  function bindShellEvents() {
    document.getElementById('logoutBtn').addEventListener('click', () => {
      Session.clear();
      location.href = 'index.html';
    });

    document.getElementById('resetBtn').addEventListener('click', () => {
      if (!confirm('سيتم إعادة تعيين البيانات التجريبية إلى الحالة الأصلية. متابعة؟')) return;
      state = Store.reset();
      Store.save(state);
      toast('success', 'تم إعادة التعيين', 'البيانات أصبحت كما كانت في البداية');
      navigate(session.role === 'admin' ? 'dashboard' : 'emp-dashboard');
    });

    document.getElementById('calGreg').addEventListener('click', () => switchCalendar('greg'));
    document.getElementById('calHijri').addEventListener('click', () => switchCalendar('hijri'));

    document.getElementById('notifBtn').addEventListener('click', e => {
      e.stopPropagation();
      document.getElementById('notifMenu').classList.toggle('show');
    });
    document.addEventListener('click', () => document.getElementById('notifMenu').classList.remove('show'));

    document.getElementById('modalClose').addEventListener('click', () => document.getElementById('modalBackdrop').classList.remove('show'));
    document.getElementById('modalBackdrop').addEventListener('click', e => {
      if (e.target.id === 'modalBackdrop') document.getElementById('modalBackdrop').classList.remove('show');
    });
  }

  function switchCalendar(c) {
    calendar = c;
    document.getElementById('calGreg').classList.toggle('active', c === 'greg');
    document.getElementById('calHijri').classList.toggle('active', c === 'hijri');
    const route = location.hash.slice(1).split('/')[0] || (session.role === 'admin' ? 'dashboard' : 'emp-dashboard');
    const ctx = location.hash.split('/')[1];
    navigate(route, ctx);
  }

  function loadNotifs() {
    document.getElementById('notifList').innerHTML = state.notifications.map(n => `
      <div class="notif">
        <div class="notif__icon">${n.icon}</div>
        <div>
          <div class="notif__text">${n.text}</div>
          <div class="notif__time">${n.time}</div>
        </div>
      </div>
    `).join('');
  }

  // ============== TOAST ==============
  window.toast = function (type, title, sub) {
    const wrap = document.getElementById('toasts');
    const el = document.createElement('div');
    el.className = `toast toast--${type}`;
    el.innerHTML = `<div><b>${title}</b>${sub ? `<small>${sub}</small>` : ''}</div>`;
    wrap.appendChild(el);
    setTimeout(() => { el.style.opacity = '0'; el.style.transform = 'translateX(20px)'; setTimeout(() => el.remove(), 300); }, 3200);
  };

})();
