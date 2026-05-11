/* ========================================================================
   إسناد / Telesak — Pure SVG charts (no library)
   ======================================================================== */

const Charts = {

  /**
   * Donut chart
   * data: [{label, value, color}]
   */
  donut(data, opts = {}) {
    const size = opts.size || 200;
    const stroke = opts.stroke || 28;
    const r = (size - stroke) / 2;
    const cx = size / 2;
    const cy = size / 2;
    const C = 2 * Math.PI * r;
    const total = data.reduce((s, d) => s + d.value, 0) || 1;

    let acc = 0;
    const segments = data.map((d, i) => {
      const frac = d.value / total;
      const dash = frac * C;
      const offset = -acc * C;
      acc += frac;
      return `<circle cx="${cx}" cy="${cy}" r="${r}" fill="none"
        stroke="${d.color}" stroke-width="${stroke}"
        stroke-dasharray="${dash} ${C - dash}" stroke-dashoffset="${offset}"
        transform="rotate(-90 ${cx} ${cy})"
        style="transition: stroke-dasharray 800ms ease ${i * 80}ms"/>`;
    }).join('');

    const legend = data.map(d => `
      <span class="chart-legend__item">
        <span class="chart-legend__dot" style="background:${d.color}"></span>
        ${d.label} <b style="margin-right: 4px; color: var(--text);">${d.value}</b>
      </span>
    `).join('');

    return `
      <div class="donut-center">
        <svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" style="display:block; margin:0 auto;">
          <circle cx="${cx}" cy="${cy}" r="${r}" fill="none" stroke="#f1f5f9" stroke-width="${stroke}"/>
          ${segments}
        </svg>
        <div class="donut-center__total">
          <div>
            <b>${total}</b>
            <span>${opts.label || 'الإجمالي'}</span>
          </div>
        </div>
      </div>
      <div class="chart-legend">${legend}</div>
    `;
  },

  /**
   * Bar chart (horizontal)
   * data: [{label, value, color?}]
   */
  bar(data, opts = {}) {
    const max = Math.max(...data.map(d => d.value), 1);
    const color = opts.color || '#3d85c6';
    const rows = data.map((d, i) => {
      const w = (d.value / max) * 100;
      return `
        <div style="display:flex; align-items:center; gap:10px; margin-bottom:14px;">
          <div style="flex: 0 0 110px; font-size: 13px; font-weight: 600;">${d.label}</div>
          <div style="flex:1; background: var(--bg); height: 24px; border-radius: 6px; overflow: hidden;">
            <div style="background: ${d.color || color}; height: 100%; width: 0;
              transition: width 800ms ease ${i * 100}ms; border-radius: 6px;
              display: flex; align-items: center; padding-right: 8px; color: white; font-size: 11px; font-weight: 700;"
              data-w="${w}">
              ${d.value}
            </div>
          </div>
        </div>`;
    }).join('');

    return `<div>${rows}</div>`;
  },

  // Animate bar widths after render
  animateBars(container) {
    requestAnimationFrame(() => {
      container.querySelectorAll('[data-w]').forEach(el => {
        el.style.width = el.dataset.w + '%';
      });
    });
  },

  /**
   * Line chart (small / sparkline)
   */
  spark(values, opts = {}) {
    const w = opts.w || 100;
    const h = opts.h || 32;
    const color = opts.color || '#3d85c6';
    const max = Math.max(...values);
    const min = Math.min(...values);
    const range = (max - min) || 1;

    const points = values.map((v, i) => {
      const x = (i / (values.length - 1)) * w;
      const y = h - ((v - min) / range) * h;
      return `${x},${y}`;
    }).join(' ');

    const area = `M0,${h} L${points.split(' ').join(' L')} L${w},${h} Z`;

    return `
      <svg width="${w}" height="${h}" viewBox="0 0 ${w} ${h}" style="display:block;">
        <defs>
          <linearGradient id="sparkGrad-${Math.random().toString(36).slice(2,8)}" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stop-color="${color}" stop-opacity=".25"/>
            <stop offset="100%" stop-color="${color}" stop-opacity="0"/>
          </linearGradient>
        </defs>
        <path d="${area}" fill="${color}" fill-opacity=".15"/>
        <polyline points="${points}" fill="none" stroke="${color}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>`;
  },

  /**
   * Vertical bar chart (months)
   */
  vbar(data, opts = {}) {
    const w = opts.w || 600;
    const h = opts.h || 200;
    const padX = 28, padY = 24, padBottom = 32;
    const max = Math.max(...data.map(d => d.value), 1);
    const cw = (w - padX * 2) / data.length;
    const bw = cw * 0.55;
    const color = opts.color || '#3d85c6';

    const bars = data.map((d, i) => {
      const x = padX + i * cw + (cw - bw) / 2;
      const bh = ((d.value / max) * (h - padY - padBottom));
      const y = h - padBottom - bh;
      return `
        <rect x="${x}" y="${h - padBottom}" width="${bw}" height="0" fill="${color}" rx="4"
          style="transition: y 700ms ease ${i * 60}ms, height 700ms ease ${i * 60}ms;">
          <animate attributeName="y" from="${h - padBottom}" to="${y}" dur=".7s" fill="freeze" begin="${i * 0.06}s"/>
          <animate attributeName="height" from="0" to="${bh}" dur=".7s" fill="freeze" begin="${i * 0.06}s"/>
        </rect>
        <text x="${x + bw/2}" y="${h - padBottom + 18}" text-anchor="middle" font-size="11" fill="#5b6b82">${d.label}</text>
        <text x="${x + bw/2}" y="${y - 6}" text-anchor="middle" font-size="11" font-weight="700" fill="#1a2537">${d.value}</text>
      `;
    }).join('');

    return `
      <svg width="100%" viewBox="0 0 ${w} ${h}" style="display:block;">
        ${bars}
      </svg>`;
  }
};

window.Charts = Charts;
