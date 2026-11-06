const especialidades = document.querySelector('#especialidades-list');
const pacientes = document.querySelector('#pacientes-list');
const buscar = document.querySelector('#buscar');
const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
async function get(url) {
  const response = await fetch(url);
  if (!response.ok) throw new Error('No se pudieron cargar los datos. Verifica la conexión con la base de datos.');
  return response.json();
}
async function cargarEspecialidades() {
  try {
    const datos = await get('/api/especialidades');
    especialidades.innerHTML = datos.length ? datos.map((e, i) => `<article class="card"><span class="number">${String(i + 1).padStart(2, '0')} /</span><h3>${escapeHtml(e.nombre)}</h3><p>${escapeHtml(e.descripcion)}</p></article>`).join('') : '<p class="notice">No hay especialidades registradas.</p>';
  } catch { especialidades.innerHTML = '<p class="notice">No se pudieron cargar las especialidades. Comprueba la base de datos.</p>'; }
}
let solicitud = 0;
async function cargarPacientes() {
  const actual = ++solicitud;
  try {
    const datos = await get('/api/pacientes?nombre=' + encodeURIComponent(buscar.value));
    if (actual !== solicitud) return;
    pacientes.innerHTML = datos.length ? datos.map(p => `<tr><td>${escapeHtml(p.nombre)}</td><td>${escapeHtml(p.historiaClinica)}</td><td>${escapeHtml(p.edad)}</td><td>${escapeHtml(p.especialidad?.nombre)}</td><td>${escapeHtml(p.motivoConsulta)}</td></tr>`).join('') : '<tr><td colspan="5">No se encontraron pacientes.</td></tr>';
  } catch { if (actual === solicitud) pacientes.innerHTML = '<tr><td colspan="5">No se pudieron cargar los pacientes. Comprueba la base de datos.</td></tr>'; }
}
buscar.addEventListener('input', cargarPacientes);
cargarEspecialidades(); cargarPacientes();
