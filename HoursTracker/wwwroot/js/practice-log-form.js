/**
 * Practice Log Form - Xử lý form thêm/sửa log luyện tập
 * Bao gồm milestone detection và popup chúc mừng
 */

document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('practiceLogForm');
    if (!form) return;
    
    // Xử lý submit form với AJAX để kiểm tra milestones
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const formData = new FormData(form);
        const submitButton = form.querySelector('button[type="submit"]');
        const originalText = submitButton.innerHTML;
        
        // Disable button và hiển thị loading
        submitButton.disabled = true;
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang xử lý...';
        
        fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': form.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => {
            // Kiểm tra nếu response là JSON (có milestones)
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return response.json();
            }
            // Nếu không phải JSON, redirect
            return response.text().then(() => ({ success: true, redirectUrl: response.url }));
        })
        .then(data => {
            if (data.success) {
                // Kiểm tra có milestones không
                if (data.milestones && data.milestones.length > 0) {
                    // Hiển thị popup chúc mừng
                    showMilestoneCelebration(data.milestones, data.redirectUrl);
                } else if (data.redirectUrl) {
                    // Redirect ngay nếu không có milestone
                    window.location.href = data.redirectUrl;
                } else {
                    // Fallback: reload trang
                    window.location.reload();
                }
            } else {
                // Nếu có lỗi, hiển thị lại form
                submitButton.disabled = false;
                submitButton.innerHTML = originalText;
                alert('Có lỗi xảy ra. Vui lòng thử lại.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            submitButton.disabled = false;
            submitButton.innerHTML = originalText;
            alert('Có lỗi xảy ra. Vui lòng thử lại.');
        });
    });
});

/**
 * Hiển thị popup chúc mừng milestone
 * @param {Array} milestones - Danh sách milestones đạt được
 * @param {string} redirectUrl - URL để redirect sau khi đóng popup
 */
function showMilestoneCelebration(milestones, redirectUrl) {
    const modal = document.getElementById('milestoneModal');
    const messageEl = document.getElementById('milestoneMessage');
    
    if (!modal || !messageEl) {
        // Nếu không có modal, redirect luôn
        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
        return;
    }
    
    // Hiển thị milestone đầu tiên (hoặc tất cả)
    if (milestones.length === 1) {
        messageEl.textContent = milestones[0].message;
    } else {
        const messages = milestones.map(m => m.message).join('\n');
        messageEl.innerHTML = messages.split('\n').join('<br>');
    }
    
    // Hiển thị modal với animation
    const bsModal = new bootstrap.Modal(modal, {
        backdrop: 'static',
        keyboard: false
    });
    bsModal.show();
    
    // Thêm animation
    const celebrationEl = modal.querySelector('.celebration-animation');
    if (celebrationEl) {
        celebrationEl.classList.add('animate-bounce');
    }
    
    // Xử lý khi đóng modal
    modal.addEventListener('hidden.bs.modal', function() {
        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    }, { once: true });
}

