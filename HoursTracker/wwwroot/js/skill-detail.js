/**
 * Skill Detail Page - Xử lý các tương tác trên trang chi tiết kỹ năng
 */

let practiceChart = null;

/**
 * Khởi tạo biểu đồ luyện tập
 * @param {object} chartData - Dữ liệu biểu đồ {labels: [], data: []}
 * @param {string} color - Màu sắc của biểu đồ
 */
function initPracticeChart(chartData, color) {
    const ctx = document.getElementById('practiceChart');
    if (!ctx) return;
    
    // Xóa biểu đồ cũ nếu có
    if (practiceChart) {
        practiceChart.destroy();
    }
    
    practiceChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartData.labels,
            datasets: [{
                label: 'Phút luyện tập',
                data: chartData.data,
                backgroundColor: color + '80', // Thêm opacity
                borderColor: color,
                borderWidth: 2,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const minutes = context.parsed.y;
                            const hours = (minutes / 60).toFixed(2);
                            return `${minutes} phút (${hours} giờ)`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return value + ' phút';
                        }
                    }
                }
            }
        }
    });
}

