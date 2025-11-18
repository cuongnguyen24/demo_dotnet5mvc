/**
 * Skill Form - Xử lý form thêm/sửa kỹ năng
 * Đồng bộ color picker với text input
 */

document.addEventListener('DOMContentLoaded', function() {
    const colorPicker = document.querySelector('input[type="color"]');
    const colorText = document.querySelector('input[type="text"][name="Color"]');
    
    if (colorPicker && colorText) {
        // Đồng bộ từ color picker sang text
        colorPicker.addEventListener('input', function() {
            colorText.value = this.value;
        });
        
        // Đồng bộ từ text sang color picker (khi user nhập)
        colorText.addEventListener('input', function() {
            // Kiểm tra format hex color
            const hexPattern = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
            if (hexPattern.test(this.value)) {
                colorPicker.value = this.value;
            }
        });
    }
});

