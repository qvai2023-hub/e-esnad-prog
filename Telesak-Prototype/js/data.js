/* ========================================================================
   إسناد / Telesak — Seed data
   Generic enterprise demo: شركة الإسناد للتقنية
   ======================================================================== */

const SEED = {
  company: {
    name: 'شركة الإسناد للتقنية',
    domain: 'esnad',
    address: 'الرياض، المملكة العربية السعودية',
    industry: 'تقنية المعلومات',
    employees: 5,
    founded: '2019'
  },

  // Login accounts (3 fields: domain + username + password)
  accounts: [
    { domain: 'esnad', username: 'admin',  password: 'admin123', role: 'admin',    empId: 'M01', name: 'م. عبدالعزيز السبيعي', title: 'مدير الشركة', email: 'admin@esnad.sa', phone: '0501110001', avatar: 1, active: true },
    { domain: 'esnad', username: 'ahmed',  password: '123456',   role: 'employee', empId: 'E01', name: 'أحمد الغامدي',     title: 'مطور برامج',  email: 'ahmed@esnad.sa', phone: '0501110002', avatar: 2, active: true },
    { domain: 'esnad', username: 'sara',   password: '123456',   role: 'employee', empId: 'E02', name: 'سارة العتيبي',      title: 'مصممة UI/UX', email: 'sara@esnad.sa',  phone: '0501110003', avatar: 3, active: true },
    { domain: 'esnad', username: 'mona',   password: '123456',   role: 'employee', empId: 'E03', name: 'منى القحطاني',      title: 'محللة بيانات', email: 'mona@esnad.sa',  phone: '0501110004', avatar: 4, active: true },
    { domain: 'esnad', username: 'yousef', password: '123456',   role: 'employee', empId: 'E04', name: 'يوسف الدوسري',      title: 'مطور Backend', email: 'yousef@esnad.sa', phone: '0501110005', avatar: 5, active: true },
    { domain: 'esnad', username: 'khalid', password: '123456',   role: 'employee', empId: 'E05', name: 'خالد الشهري',       title: 'موظف موقوف',   email: 'khalid@esnad.sa', phone: '0501110006', avatar: 6, active: false }
  ],

  // Projects
  projects: [
    { id: 'P01', name: 'تطوير منصة إسناد ٢.٠', desc: 'إعادة بناء المنصة بتقنيات حديثة وتحسين الأداء', status: 'active',  start: '2026-01-15', end: '2026-06-30', color: '#3d85c6' },
    { id: 'P02', name: 'حملة التسويق الرقمي ٢٠٢٦',  desc: 'حملة شاملة عبر وسائل التواصل ومحركات البحث',     status: 'active',  start: '2026-02-01', end: '2026-04-30', color: '#10b981' },
    { id: 'P03', name: 'تقرير الأداء السنوي',         desc: 'تجميع وتحليل أداء الفريق للعام السابق',         status: 'done',    start: '2026-01-01', end: '2026-02-15', color: '#8b5cf6' },
    { id: 'P04', name: 'إعادة تصميم الموقع الإلكتروني', desc: 'تحديث الهوية البصرية وتحسين تجربة المستخدم',  status: 'active',  start: '2026-03-01', end: '2026-05-15', color: '#f59e0b' },
    { id: 'P05', name: 'نظام إدارة العملاء (CRM)',    desc: 'بناء نظام داخلي لإدارة علاقات العملاء',         status: 'planning', start: '2026-04-01', end: '2026-08-30', color: '#06b6d4' }
  ],

  // 25 tasks distributed
  tasks: [
    // Project 1 — Esnad 2.0
    { id: 'T01', title: 'تصميم قاعدة البيانات الجديدة', projectId: 'P01', empId: 'E04', status: 'done',     priority: 'high', start: '2026-02-01', due: '2026-02-15', timeSpent: '32:00', comments: 4, attachments: 2 },
    { id: 'T02', title: 'تطوير API الخاص بالمستخدمين',   projectId: 'P01', empId: 'E04', status: 'progress', priority: 'high', start: '2026-02-16', due: '2026-03-10', timeSpent: '18:30', comments: 3, attachments: 1 },
    { id: 'T03', title: 'بناء واجهة لوحة التحكم',          projectId: 'P01', empId: 'E01', status: 'progress', priority: 'high', start: '2026-02-20', due: '2026-03-15', timeSpent: '22:15', comments: 5, attachments: 3 },
    { id: 'T04', title: 'تكامل SignalR للإشعارات الفورية', projectId: 'P01', empId: 'E04', status: 'new',      priority: 'med',  start: '2026-03-15', due: '2026-04-05', timeSpent: '0:00',  comments: 1, attachments: 0 },
    { id: 'T05', title: 'كتابة وحدات الاختبار',            projectId: 'P01', empId: 'E01', status: 'new',      priority: 'med',  start: '2026-03-20', due: '2026-04-15', timeSpent: '0:00',  comments: 0, attachments: 0 },
    { id: 'T06', title: 'مراجعة أمان النظام', projectId: 'P01', empId: 'E03', status: 'late', priority: 'high', start: '2026-02-10', due: '2026-03-01', timeSpent: '8:00', comments: 2, attachments: 0 },

    // Project 2 — Marketing
    { id: 'T07', title: 'إعداد المحتوى لشهر مارس',         projectId: 'P02', empId: 'E02', status: 'done',     priority: 'med',  start: '2026-02-15', due: '2026-02-28', timeSpent: '20:00', comments: 6, attachments: 5 },
    { id: 'T08', title: 'تصميم منشورات تويتر/إنستجرام',  projectId: 'P02', empId: 'E02', status: 'progress', priority: 'med',  start: '2026-03-01', due: '2026-03-20', timeSpent: '12:30', comments: 4, attachments: 8 },
    { id: 'T09', title: 'تحليل أداء الحملة الأسبوعي',      projectId: 'P02', empId: 'E03', status: 'progress', priority: 'med',  start: '2026-03-05', due: '2026-03-25', timeSpent: '6:00',  comments: 2, attachments: 1 },
    { id: 'T10', title: 'حملة Google Ads',                projectId: 'P02', empId: 'E03', status: 'new',      priority: 'high', start: '2026-03-15', due: '2026-04-15', timeSpent: '0:00',  comments: 0, attachments: 0 },
    { id: 'T11', title: 'تقرير ROI للحملة',                  projectId: 'P02', empId: 'E03', status: 'new',      priority: 'low',  start: '2026-04-01', due: '2026-04-25', timeSpent: '0:00',  comments: 0, attachments: 0 },

    // Project 3 — Annual Report
    { id: 'T12', title: 'جمع بيانات الموظفين',              projectId: 'P03', empId: 'E03', status: 'approved', priority: 'med',  start: '2026-01-05', due: '2026-01-15', timeSpent: '14:00', comments: 3, attachments: 2 },
    { id: 'T13', title: 'تحليل KPIs الفريق',                projectId: 'P03', empId: 'E03', status: 'approved', priority: 'high', start: '2026-01-16', due: '2026-01-31', timeSpent: '24:00', comments: 4, attachments: 3 },
    { id: 'T14', title: 'كتابة التقرير النهائي',             projectId: 'P03', empId: 'E03', status: 'approved', priority: 'high', start: '2026-02-01', due: '2026-02-15', timeSpent: '30:00', comments: 7, attachments: 4 },

    // Project 4 — Website redesign
    { id: 'T15', title: 'بحث المستخدمين والمنافسين',         projectId: 'P04', empId: 'E02', status: 'done',     priority: 'high', start: '2026-03-01', due: '2026-03-10', timeSpent: '15:00', comments: 4, attachments: 3 },
    { id: 'T16', title: 'تصميم الـ Wireframes',           projectId: 'P04', empId: 'E02', status: 'progress', priority: 'high', start: '2026-03-11', due: '2026-03-30', timeSpent: '10:00', comments: 5, attachments: 6 },
    { id: 'T17', title: 'تصميم Mockups عالية الدقة',        projectId: 'P04', empId: 'E02', status: 'new',      priority: 'med',  start: '2026-04-01', due: '2026-04-20', timeSpent: '0:00',  comments: 0, attachments: 0 },
    { id: 'T18', title: 'تطوير Frontend (React)',           projectId: 'P04', empId: 'E01', status: 'new',      priority: 'med',  start: '2026-04-15', due: '2026-05-15', timeSpent: '0:00',  comments: 0, attachments: 0 },

    // Project 5 — CRM
    { id: 'T19', title: 'دراسة المتطلبات والحوكمة', projectId: 'P05', empId: 'M01', status: 'progress', priority: 'high', start: '2026-04-01', due: '2026-04-15', timeSpent: '4:00', comments: 1, attachments: 1 },
    { id: 'T20', title: 'اختيار التقنيات المناسبة',           projectId: 'P05', empId: 'E04', status: 'new',      priority: 'med',  start: '2026-04-10', due: '2026-04-25', timeSpent: '0:00',  comments: 0, attachments: 0 },

    // Standalone / personal tasks for ahmed (employee demo)
    { id: 'T21', title: 'حضور اجتماع الفريق الأسبوعي',        projectId: 'P01', empId: 'E01', status: 'new',      priority: 'low',  start: '2026-05-04', due: '2026-05-04', timeSpent: '0:00',  comments: 0, attachments: 0 },
    { id: 'T22', title: 'مراجعة كود زميل (Code Review)',       projectId: 'P01', empId: 'E01', status: 'progress', priority: 'med',  start: '2026-05-02', due: '2026-05-05', timeSpent: '1:30',  comments: 2, attachments: 0 },
    { id: 'T23', title: 'تحديث وثيقة API التقنية',           projectId: 'P01', empId: 'E01', status: 'late',     priority: 'med',  start: '2026-04-20', due: '2026-05-01', timeSpent: '3:00',  comments: 1, attachments: 1 },
    { id: 'T24', title: 'إصلاح bug في تسجيل الدخول',         projectId: 'P01', empId: 'E01', status: 'progress', priority: 'high', start: '2026-05-03', due: '2026-05-06', timeSpent: '2:15',  comments: 3, attachments: 0 },
    { id: 'T25', title: 'تجهيز عرض تقديمي للعميل',           projectId: 'P02', empId: 'E02', status: 'new',      priority: 'high', start: '2026-05-05', due: '2026-05-08', timeSpent: '0:00',  comments: 0, attachments: 0 }
  ],

  // Comments
  comments: [
    { taskId: 'T03', empId: 'M01', text: 'يا فريق، لا تنسوا تطبيق الـ design system الموحد على كل الصفحات', date: '2026-04-28 09:15' },
    { taskId: 'T03', empId: 'E01', text: 'تمام، خلصت من صفحة الدخول والداشبورد. الباقي قائمة المهام والتقارير', date: '2026-04-29 14:22' },
    { taskId: 'T03', empId: 'E02', text: 'رفعت الـ Mockups المحدّثة في المرفقات', date: '2026-04-30 11:05' },
    { taskId: 'T03', empId: 'M01', text: 'ممتاز، الشكل النهائي أصبح احترافي جداً 👍', date: '2026-05-01 16:40' },
    { taskId: 'T03', empId: 'E01', text: 'باقي تجهيز الـ responsive للجوال، أتوقع ينتهي بكرة', date: '2026-05-02 10:30' },
    { taskId: 'T08', empId: 'M01', text: 'تذكروا الالتزام بالألوان الجديدة من دليل الهوية', date: '2026-03-10 09:00' },
    { taskId: 'T08', empId: 'E02', text: 'بدأت في تصميم الـ templates لشهر مارس', date: '2026-03-12 13:15' }
  ],

  // Activity log
  activities: [
    { type: 'create',   icon: '➕', color: 'blue',  by: 'M01', what: 'أنشأ مهمة', target: 'تجهيز عرض تقديمي للعميل', date: '2026-05-04 10:15' },
    { type: 'complete', icon: '✓',  color: 'green', by: 'E02', what: 'أنهى مهمة',  target: 'بحث المستخدمين والمنافسين', date: '2026-05-04 09:42' },
    { type: 'comment',  icon: '💬', color: 'blue',  by: 'E01', what: 'علّق على',   target: 'بناء واجهة لوحة التحكم', date: '2026-05-03 16:30' },
    { type: 'attach',   icon: '📎', color: 'amber', by: 'E02', what: 'رفع مرفقاً على', target: 'تصميم Wireframes', date: '2026-05-03 14:10' },
    { type: 'accept',   icon: '✓',  color: 'green', by: 'E04', what: 'قبل مهمة',     target: 'تطوير API المستخدمين', date: '2026-05-03 11:05' },
    { type: 'late',     icon: '⚠',  color: 'red',   by: 'E01', what: 'تأخّر تسليم', target: 'تحديث وثيقة API التقنية', date: '2026-05-02 23:59' },
    { type: 'create',   icon: '➕', color: 'blue',  by: 'M01', what: 'أنشأ مشروع',   target: 'نظام إدارة العملاء (CRM)', date: '2026-05-02 09:00' },
    { type: 'reassign', icon: '↻',  color: 'amber', by: 'M01', what: 'أعاد إسناد مهمة', target: 'كتابة وحدات الاختبار', date: '2026-05-01 17:20' },
    { type: 'login',    icon: '→',  color: 'blue',  by: 'E03', what: 'سجّلت دخول',     target: '', date: '2026-05-01 08:01' },
    { type: 'complete', icon: '✓',  color: 'green', by: 'E03', what: 'أنهت مهمة',     target: 'إعداد المحتوى لشهر مارس', date: '2026-04-30 15:55' },
    { type: 'create',   icon: '➕', color: 'blue',  by: 'M01', what: 'أضاف موظفاً',     target: 'منى القحطاني', date: '2026-04-29 10:30' },
    { type: 'reject',   icon: '✗',  color: 'red',   by: 'M01', what: 'رفض إنجاز مهمة', target: 'حملة Google Ads', date: '2026-04-28 14:00' }
  ],

  // Attendance (last 7 days, ahmed example)
  attendance: [
    { empId: 'E01', date: '2026-05-04', in: '08:32', out: null,    hours: '—' },
    { empId: 'E01', date: '2026-05-03', in: '08:15', out: '17:05', hours: '8:50' },
    { empId: 'E01', date: '2026-05-02', in: '08:45', out: '16:50', hours: '8:05' },
    { empId: 'E01', date: '2026-05-01', in: '09:00', out: '17:30', hours: '8:30' },
    { empId: 'E01', date: '2026-04-30', in: '08:25', out: '17:15', hours: '8:50' },
    { empId: 'E01', date: '2026-04-29', in: '08:40', out: '17:00', hours: '8:20' },
    { empId: 'E01', date: '2026-04-28', in: '08:30', out: '16:55', hours: '8:25' }
  ],

  // Notifications (top right bell)
  notifications: [
    { icon: '✓', text: '<b>سارة العتيبي</b> أنهت مهمة "بحث المستخدمين"', time: 'منذ 18 دقيقة' },
    { icon: '💬', text: '<b>أحمد الغامدي</b> علّق على "بناء واجهة لوحة التحكم"', time: 'منذ ساعة' },
    { icon: '📎', text: 'تم رفع 3 مرفقات على مشروع "إعادة تصميم الموقع"', time: 'منذ ساعتين' },
    { icon: '⚠', text: 'مهمة "مراجعة أمان النظام" متأخرة', time: 'أمس' },
    { icon: '➕', text: 'تم إنشاء مشروع جديد "نظام CRM"', time: 'أمس' }
  ]
};

/* ----------------- localStorage helpers ----------------- */
const STORE_KEY = 'esnad_demo_v1';

const Store = {
  load() {
    try {
      const raw = localStorage.getItem(STORE_KEY);
      if (raw) return JSON.parse(raw);
    } catch (e) {}
    return JSON.parse(JSON.stringify(SEED));
  },
  save(state) {
    localStorage.setItem(STORE_KEY, JSON.stringify(state));
  },
  reset() {
    localStorage.removeItem(STORE_KEY);
    sessionStorage.removeItem('esnad_session');
    return JSON.parse(JSON.stringify(SEED));
  }
};

/* ----------------- session ----------------- */
const Session = {
  get() {
    try { return JSON.parse(sessionStorage.getItem('esnad_session') || 'null'); }
    catch (e) { return null; }
  },
  set(account) {
    sessionStorage.setItem('esnad_session', JSON.stringify(account));
  },
  clear() {
    sessionStorage.removeItem('esnad_session');
  }
};

/* ----------------- helpers ----------------- */
const STATUS_LABELS = {
  new: 'جديدة', progress: 'جاري العمل', done: 'منتهية',
  approved: 'معتمدة', late: 'متأخرة', paused: 'معلقة',
  rejected: 'مرفوضة', archived: 'مؤرشفة'
};
const PRIORITY_LABELS = { high: 'عالية', med: 'متوسطة', low: 'منخفضة' };

function fmtDateAr(iso, calendar = 'greg') {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d)) return iso;
  if (calendar === 'hijri') {
    try {
      return new Intl.DateTimeFormat('ar-SA-u-ca-islamic-umalqura', {
        year: 'numeric', month: 'short', day: 'numeric'
      }).format(d);
    } catch (e) {}
  }
  return new Intl.DateTimeFormat('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' }).format(d);
}
function isLate(task) { return task.status !== 'done' && task.status !== 'approved' && new Date(task.due) < new Date('2026-05-04'); }

window.SEED = SEED;
window.Store = Store;
window.Session = Session;
window.STATUS_LABELS = STATUS_LABELS;
window.PRIORITY_LABELS = PRIORITY_LABELS;
window.fmtDateAr = fmtDateAr;
window.isLate = isLate;
