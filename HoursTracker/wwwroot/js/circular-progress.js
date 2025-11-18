/**
 * Circular Progress - Hiển thị tiến độ dạng vòng tròn
 * Sử dụng SVG để vẽ progress ring
 */

document.addEventListener('DOMContentLoaded', function() {
    // Khởi tạo tất cả circular progress
    initAllCircularProgress();
});

/**
 * Khởi tạo tất cả các circular progress trên trang
 */
function initAllCircularProgress() {
    const progressElements = document.querySelectorAll('.circular-progress, .circular-progress-large');
    
    progressElements.forEach(element => {
        const percentage = parseFloat(element.getAttribute('data-percentage')) || 0;
        const color = element.getAttribute('data-color') || '#007bff';
        
        updateCircularProgress(element, percentage, color);
    });
}

/**
 * Cập nhật circular progress
 * @param {HTMLElement} element - Element chứa progress
 * @param {number} percentage - Phần trăm (0-100)
 * @param {string} color - Màu sắc
 */
function updateCircularProgress(element, percentage, color) {
    const circle = element.querySelector('.progress-ring-circle');
    const percentageText = element.querySelector('.progress-percentage');
    
    if (!circle) return;
    
    // Tính toán chu vi và offset
    const radius = circle.getAttribute('r');
    const circumference = 2 * Math.PI * radius;
    
    // Thiết lập stroke-dasharray và stroke-dashoffset
    circle.style.strokeDasharray = `${circumference} ${circumference}`;
    circle.style.strokeDashoffset = circumference;
    
    // Tính toán offset dựa trên percentage
    const offset = circumference - (percentage / 100) * circumference;
    
    // Animation
    setTimeout(() => {
        circle.style.transition = 'stroke-dashoffset 1s ease-in-out';
        circle.style.strokeDashoffset = offset;
    }, 100);
    
    // Cập nhật màu sắc
    circle.setAttribute('stroke', color);
    
    // Cập nhật text nếu có
    if (percentageText) {
        percentageText.textContent = percentage.toFixed(1) + '%';
    }
}

