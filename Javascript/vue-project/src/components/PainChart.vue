<template>
  <canvas ref="chartRef" height="120"></canvas>
</template>

<script setup>
import { onMounted, watch, ref } from 'vue';
import Chart from 'chart.js/auto';

const props = defineProps({ logs: { type: Array, default: () => [] } });
const chartRef = ref(null);
let chart = null;

onMounted(() => {
  chart = new Chart(chartRef.value, {
    type: 'line',
    data: {
      labels: props.logs.map(l => new Date(l.createdAt).toLocaleDateString()),
      datasets: [{
        label: 'Pijn (0-10)',
        data: props.logs.map(l => l.value),
        borderColor: '#ef4444',
        backgroundColor: 'rgba(239,68,68,0.1)',
        tension: 0.2,
      }],
    },
    options: { responsive: true, scales: { y: { min: 0, max: 10 } } },
  });
});

watch(() => props.logs, (nv) => {
  if (!chart) return;
  chart.data.labels = nv.map(l => new Date(l.createdAt).toLocaleDateString());
  chart.data.datasets[0].data = nv.map(l => l.value);
  chart.update();
});
</script>
