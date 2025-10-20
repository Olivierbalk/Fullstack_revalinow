<script setup>
import { ref, computed, onMounted, nextTick, watch } from 'vue'


const agenda = ref([])
const loading = ref(true)
const error = ref(null)

const showPast = ref(false)
const showOlderThanYear = ref(false)
const viewMode = ref('next') // 'next', 'prev', or 'all'


function formatDate(dateStr) {
  const date = new Date(dateStr)
  if (isNaN(date)) return dateStr
  return date.toLocaleString()
}

const now = new Date()
const oneYearAgo = new Date()
oneYearAgo.setFullYear(now.getFullYear() - 1)

const filteredAgenda = computed(() => {
  // Create inclusive day-range windows and normalize times so comparisons are reliable
  const start = new Date()
  start.setHours(0, 0, 0, 0) // start of today

  const end = new Date(start)
  end.setDate(end.getDate() + 30)
  end.setHours(23, 59, 59, 999) // end of day 30 days from now

  const prevEnd = new Date(start)
  prevEnd.setHours(23, 59, 59, 999)
  prevEnd.setDate(prevEnd.getDate() - 1) // yesterday end

  const prevStart = new Date(prevEnd)
  prevStart.setDate(prevStart.getDate() - 29) // 30-day window ending yesterday

  const filtered = agenda.value.filter(item => {
    const d = new Date(item.date)
    if (isNaN(d)) return false // skip invalid dates
    if (viewMode.value === 'next') {
      return d >= start && d <= end
    } else if (viewMode.value === 'prev') {
      return d >= prevStart && d <= prevEnd
    } else {
      return true
    }
  })

  // Ensure a stable chronological order
  return filtered.sort((a, b) => new Date(a.date) - new Date(b.date))
})

function groupByDate(items) {
  const groups = {}
  items.forEach(item => {
    const dateKey = new Date(item.date).toLocaleDateString('nl-NL', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
    if (!groups[dateKey]) groups[dateKey] = []
    groups[dateKey].push(item)
  })
  return groups
}

onMounted(async () => {
  try {
    // Replace with your actual API endpoint
    const res = await fetch('http://192.168.178.134:5050/Appointment')
    if (!res.ok) throw new Error('Failed to fetch agenda')
    const data = await res.json()
    if (!Array.isArray(data) || data.length === 0) {
      error.value = 'Geen afspraken gevonden.'
      agenda.value = []
    } else {
      // Filter out items without valid dates and sort chronologically
      const valid = data.filter(item => {
        const d = new Date(item.date)
        if (isNaN(d)) {
          console.warn('Skipping invalid date entry:', item)
          return false
        }
        return true
      }).sort((a, b) => new Date(a.date) - new Date(b.date))
      agenda.value = valid
      console.log('Fetched and normalized agenda:', valid)
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
})

// Responsive agenda layout for 'all' mode
const isAllMode = computed(() => viewMode.value === 'all')

// Helper: get all unique time slots (rounded to 15 min)
function getTimeSlots(items) {
  const slots = new Set()
  items.forEach(item => {
    const d = new Date(item.date)
    const h = d.getHours().toString().padStart(2, '0')
    const m = (Math.floor(d.getMinutes() / 15) * 15).toString().padStart(2, '0')
    slots.add(`${h}:${m}`)
  })
  return Array.from(slots).sort()
}

// Helper: get all unique days in the data (ma-zo)
function getDays(items) {
  const days = new Set()
  items.forEach(item => {
    const d = new Date(item.date)
    days.add(d.toLocaleDateString('nl-NL', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit' }))
  })
  return Array.from(days).sort((a, b) => new Date(a) - new Date(b))
}

// Helpers voor groeperen per jaar, maand, week
function groupByYearMonthWeek(items) {
  const groups = {}
  items.forEach(item => {
    const d = new Date(item.date)
    const year = d.getFullYear()
    const month = d.toLocaleString('nl-NL', { month: 'long' })
    // Weeknummer berekenen (ISO week)
    const temp = new Date(d.getTime())
    temp.setHours(0, 0, 0, 0)
    temp.setDate(temp.getDate() + 4 - (temp.getDay() || 7))
    const yearStart = new Date(temp.getFullYear(), 0, 1)
    const weekNo = Math.ceil((((temp - yearStart) / 86400000) + 1) / 7)
    if (!groups[year]) groups[year] = {}
    if (!groups[year][month]) groups[year][month] = {}
    if (!groups[year][month][weekNo]) groups[year][month][weekNo] = []
    groups[year][month][weekNo].push(item)
  })
  return groups
}

// Weekrooster helpers voor 'alles' mode
const weekStart = ref(getStartOfWeek(new Date()))

function getStartOfWeek(date) {
  const d = new Date(date)
  d.setHours(0, 0, 0, 0)
  d.setDate(d.getDate() - ((d.getDay() + 6) % 7)) // Maandag als eerste dag
  return d
}

function getWeekDays(start) {
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(start)
    d.setDate(d.getDate() + i)
    return d
  })
}

function getTimeBlocks(start = 8, end = 20) {
  const blocks = []
  for (let h = start; h <= end; h++) {
    blocks.push(`${h}:00`)
    blocks.push(`${h}:30`)
  }
  return blocks
}

// Helper: alle afspraken van de getoonde week
function getEventsForWeek(weekStart) {
  const start = new Date(weekStart)
  const end = new Date(weekStart)
  end.setDate(start.getDate() + 7)
  return agenda.value.filter(item => {
    const d = new Date(item.date)
    return d >= start && d < end
  })
}

function getEventsForSlot(day, time) {
  return filteredAgenda.value.filter(item => {
    const d = new Date(item.date)
    return d.toDateString() === day.toDateString() &&
      d.getHours() === parseInt(time.split(':')[0]) &&
      d.getMinutes() >= parseInt(time.split(':')[1]) && d.getMinutes() < parseInt(time.split(':')[1]) + 30
  })
}

function prevWeek() {
  weekStart.value.setDate(weekStart.value.getDate() - 7)
  weekStart.value = new Date(weekStart.value)
}
function nextWeek() {
  weekStart.value.setDate(weekStart.value.getDate() + 7)
  weekStart.value = new Date(weekStart.value)
}
function resetWeek() {
  weekStart.value = getStartOfWeek(new Date())
}

// State for month scroll menu
const monthMenu = ref([])
const currentMonthKey = ref('')
const monthRefs = ref({})

function getMonthKeys(groups) {
  // Returns array of { year, month, key } for scroll menu
  const keys = []
  Object.entries(groups).forEach(([year, months]) => {
    Object.keys(months).forEach(month => {
      keys.push({ year, month, key: `${year}-${month}` })
    })
  })
  return keys
}

function scrollToMonth(key) {
  nextTick(() => {
    if (monthRefs.value[key]) {
      monthRefs.value[key].scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  })
}

onMounted(() => {
  // Set currentMonthKey to current year-month
  const now = new Date()
  const year = now.getFullYear()
  const month = now.toLocaleString('nl-NL', { month: 'long' })
  currentMonthKey.value = `${year}-${month}`
})

watch([filteredAgenda, isAllMode], () => {
  if (isAllMode.value) {
    nextTick(() => scrollToMonth(currentMonthKey.value))
  }
})
</script>

<template>
  <div :class="['agenda-gcal-container', { 'agenda-gcal-full': isAllMode }]">
    <h2 class="agenda-gcal-title">Agenda</h2>
    <div v-if="loading" class="agenda-gcal-loading">Loading...</div>
    <div v-else-if="error" class="agenda-gcal-error">Error: {{ error }}</div>
    <div v-else>
      <div class="agenda-gcal-toggles">
        <button
          class="agenda-gcal-togglebtn"
          :class="{ active: viewMode === 'next' }"
          @click="viewMode = viewMode === 'next' ? 'all' : 'next'"
        >
          {{ viewMode === 'next' ? 'Toon alle afspraken' : 'Toon komende maand' }}
        </button>
        <button
          class="agenda-gcal-togglebtn"
          :class="{ active: viewMode === 'prev' }"
          @click="viewMode = viewMode === 'prev' ? 'all' : 'prev'"
        >
          {{ viewMode === 'prev' ? 'Toon alle afspraken' : 'Toon afgelopen maand' }}
        </button>
      </div>
      <div v-if="Object.keys(groupByDate(filteredAgenda)).length === 0" class="agenda-gcal-empty">Geen afspraken in deze periode.</div>
      <div v-else>
        <div v-if="isAllMode" class="agenda-gcal-hierarchical">
          <!-- Month scroll menu -->
          <div class="agenda-gcal-monthmenu">
            <button
              v-for="m in getMonthKeys(groupByYearMonthWeek(filteredAgenda))"
              :key="m.key"
              :class="['agenda-gcal-monthbtn', { active: m.key === currentMonthKey } ]"
              @click="scrollToMonth(m.key)"
            >
              {{ m.month }} {{ m.year }}
            </button>
          </div>
          <div v-for="(months, year) in groupByYearMonthWeek(filteredAgenda)" :key="year" class="agenda-gcal-year">
            <div class="agenda-gcal-year-header">{{ year }}</div>
            <div v-for="(weeks, month) in months" :key="month" class="agenda-gcal-month" :ref="el => monthRefs.value[`${year}-${month}`] = el">
              <div class="agenda-gcal-month-header">{{ month }}</div>
              <div v-for="(items, week) in weeks" :key="week" class="agenda-gcal-week">
                <div class="agenda-gcal-week-header">Week {{ week }}</div>
                <ul class="agenda-gcal-list">
                  <li v-for="item in items" :key="item.id" class="agenda-gcal-item" :data-type="item.name">
                    <div class="agenda-gcal-event-bar"></div>
                    <div class="agenda-gcal-event-content">
                      <span class="agenda-gcal-event-name">{{ item.name }}</span>
                      <span class="agenda-gcal-event-time">{{ formatDate(item.date) }}</span>
                    </div>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
        <div v-else>
          <div v-for="(items, date) in groupByDate(filteredAgenda)" :key="date" class="agenda-gcal-day">
            <div class="agenda-gcal-date">{{ date }}</div>
            <ul class="agenda-gcal-list">
              <li v-for="item in items" :key="item.id" class="agenda-gcal-item" :data-type="item.name">
                <div class="agenda-gcal-event-bar"></div>
                <div class="agenda-gcal-event-content">
                  <span class="agenda-gcal-event-name">{{ item.name }}</span>
                  <span class="agenda-gcal-event-time">{{ formatDate(item.date) }}</span>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.agenda-gcal-container {
  max-width: 520px;
  margin: 2.5rem auto;
  background: #f8fafc;
  border-radius: 18px;
  box-shadow: 0 4px 16px rgba(60,64,67,0.08);
  padding: 2.5rem 1.5rem 2rem 1.5rem;
  font-family: 'Segoe UI', 'Roboto', Arial, sans-serif;
  transition: max-width 0.3s, margin 0.3s;
}
.agenda-gcal-full {
  max-width: 100vw;
  margin: 0;
  border-radius: 0;
  padding: 2.5rem 2vw 2rem 2vw;
}
.agenda-gcal-toggles {
  display: flex;
  gap: 2rem;
  justify-content: center;
  margin-bottom: 1.5rem;
}
.agenda-gcal-togglebtn {
  border: none;
  border-radius: 6px;
  padding: 0.5rem 1.2rem;
  font-size: 1rem;
  cursor: pointer;
  background: #e3eafc;
  color: #1967d2;
  font-weight: 500;
  transition: background 0.2s, color 0.2s;
}
.agenda-gcal-togglebtn.active, .agenda-gcal-togglebtn:active {
  background: #1967d2;
  color: #fff;
}
.agenda-gcal-day {
  margin-bottom: 2.2rem;
}
.agenda-gcal-date {
  font-weight: bold;
  font-size: 1.1rem;
  color: #2c3e50;
  margin-bottom: 0.5rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.2rem;
}
.agenda-gcal-empty {
  text-align: center;
  color: #888;
  font-size: 1.1rem;
  margin: 2.5rem 0;
}
.agenda-gcal-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
  gap: 2rem 2.5rem;
  margin-top: 2.5rem;
}
.agenda-gcal-griditem {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(60,64,67,0.10);
  display: flex;
  align-items: stretch;
  min-height: 56px;
  border-left: 6px solid #1967d2;
  margin-bottom: 0;
  position: relative;
  transition: box-shadow 0.2s, border-color 0.2s;
}
.agenda-gcal-griditem[data-type*="Fysio"] {
  border-left-color: #43a047;
}
.agenda-gcal-griditem[data-type*="Dokter"] {
  border-left-color: #e53935;
}
.agenda-gcal-griditem[data-type*="Revalidatie"] {
  border-left-color: #fbc02d;
}
.agenda-gcal-event-bar {
  width: 6px;
  background: transparent;
  border-radius: 0 6px 6px 0;
  margin-right: 0.9rem;
}
.agenda-gcal-event-content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 0.6rem 1rem 0.6rem 0;
  flex: 1;
}
.agenda-gcal-event-name {
  font-size: 1.15rem;
  color: #222;
  font-weight: 500;
  margin-bottom: 2px;
}
.agenda-gcal-event-time {
  font-size: 1.05rem;
  color: #1967d2;
  font-family: monospace;
}
.agenda-gcal-roster {
  width: 100vw;
  overflow-x: auto;
  margin-top: 2.5rem;
}
.agenda-gcal-roster-header, .agenda-gcal-roster-row {
  display: grid;
  grid-template-columns: 80px repeat(auto-fit, minmax(180px, 1fr));
  min-width: 900px;
}
.agenda-gcal-roster-header {
  font-weight: bold;
  background: #e3eafc;
  color: #1967d2;
  border-radius: 8px 8px 0 0;
}
.agenda-gcal-roster-timecol {
  background: #f8fafc;
  color: #888;
  padding: 0.5rem 0.7rem;
  border-right: 1px solid #e0e0e0;
  min-width: 70px;
  text-align: right;
}
.agenda-gcal-roster-daycol {
  min-height: 48px;
  border-right: 1px solid #e0e0e0;
  border-bottom: 1px solid #e0e0e0;
  padding: 0.5rem 0.7rem;
  background: #fff;
  vertical-align: top;
}
.agenda-gcal-roster-daylabel {
  text-align: center;
  background: #e3eafc;
  color: #1967d2;
  border-bottom: 2px solid #1967d2;
}
.agenda-gcal-roster-event {
  background: #e8f0fe;
  border-radius: 6px;
  margin-bottom: 0.3rem;
  padding: 0.3rem 0.5rem;
  color: #174ea6;
  font-size: 1rem;
  box-shadow: 0 1px 4px rgba(60,64,67,0.08);
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}
.agenda-gcal-hierarchical {
  width: 100vw;
  max-width: 100vw;
  margin: 0;
  padding: 0 2vw 2vw 2vw;
}
.agenda-gcal-year-header {
  font-size: 2rem;
  color: #174ea6;
  font-weight: bold;
  margin-top: 2.5rem;
  margin-bottom: 1.2rem;
  border-bottom: 2px solid #1967d2;
  padding-bottom: 0.3rem;
}
.agenda-gcal-month-header {
  font-size: 1.3rem;
  color: #1967d2;
  font-weight: 600;
  margin-top: 1.5rem;
  margin-bottom: 0.7rem;
  border-bottom: 1px solid #e0e0e0;
  padding-bottom: 0.2rem;
}
.agenda-gcal-week-header {
  font-size: 1.1rem;
  color: #333;
  font-weight: 500;
  margin-top: 1rem;
  margin-bottom: 0.3rem;
}
.agenda-gcal-monthmenu {
  display: flex;
  overflow-x: auto;
  gap: 0.5rem;
  margin-bottom: 1.2rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #e0e0e0;
  background: #f8fafc;
}
.agenda-gcal-monthbtn {
  border: none;
  background: #e3eafc;
  color: #1967d2;
  font-weight: 500;
  border-radius: 6px;
  padding: 0.4rem 1.1rem;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
}
.agenda-gcal-monthbtn.active, .agenda-gcal-monthbtn:active {
  background: #1967d2;
  color: #fff;
}
@media (max-width: 600px) {
  .agenda-gcal-container {
    padding: 1rem 0.2rem;
  }
  .agenda-gcal-title {
    font-size: 1.3rem;
  }
}
</style>

<style scoped>
.agenda-simple-container {
  max-width: 500px;
  margin: 2rem auto;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
  padding: 2rem 1.5rem;
}
.agenda-day {
  margin-bottom: 2rem;
}
.agenda-date {
  font-weight: bold;
  font-size: 1.1rem;
  color: #2c3e50;
  margin-bottom: 0.5rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.2rem;
}
.agenda-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
.agenda-item {
  display: flex;
  align-items: center;
  padding: 0.5rem 0;
  border-bottom: 1px dashed #e0e0e0;
}
.agenda-item:last-child {
  border-bottom: none;
}
.agenda-time {
  font-family: monospace;
  color: #1976d2;
  margin-right: 1rem;
  min-width: 60px;
}
.agenda-name {
  font-size: 1rem;
}
.agenda-gcal-weekbtn {
  border: none;
  border-radius: 6px;
  padding: 0.4rem 1.1rem;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.2s;
}
.agenda-gcal-weekbtn:hover {
  background: #174ea6;
}
.agenda-gcal-weeklabel {
  font-size: 1.08rem;
  color: #333;
  font-weight: 500;
}
.agenda-gcal-empty {
  text-align: center;
  color: #888;
  font-size: 1.1rem;
  margin: 2.5rem 0;
}
.agenda-gcal-day {
  display: flex;
  align-items: flex-start;
}
.agenda-empty-week {
  text-align: center;
  color: #888;
  font-size: 1.1rem;
  margin: 2.5rem 0;
}
</style>
