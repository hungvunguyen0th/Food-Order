// POS JAVASCRIPT
let currentOrder = [];
let currentPaymentMethod = 'cash';

// Filter products by category
function filterCategory(category) {
    const products = document.querySelectorAll('.product-card');
    const buttons = document.querySelectorAll('. category-btn');

    // Update active button
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    // Filter products
    products.forEach(product => {
        if (category === 'all' || product.dataset.category === category) {
            product.style.display = 'block';
        } else {
            product.style.display = 'none';
        }
    });
}

// Search products
function searchProducts() {
    const searchTerm = document.getElementById('searchProduct').value.toLowerCase();
    const products = document.querySelectorAll('.product-card');

    products.forEach(product => {
        const name = product.dataset.name.toLowerCase();
        if (name.includes(searchTerm)) {
            product.style.display = 'block';
        } else {
            product.style.display = 'none';
        }
    });
}

// Add item to order
function addToOrder(id, name, price) {
    const existingItem = currentOrder.find(item => item.id === id);

    if (existingItem) {
        existingItem.quantity++;
    } else {
        currentOrder.push({
            id: id,
            name: name,
            price: price,
            quantity: 1
        });
    }

    renderOrder();
    showNotification(`Đã thêm ${name}`, 'success');
}

// Remove item from order
function removeFromOrder(id) {
    currentOrder = currentOrder.filter(item => item.id !== id);
    renderOrder();
    showNotification('Đã xóa món khỏi đơn hàng', 'info');
}

// Update quantity
function updateQuantity(id, delta) {
    const item = currentOrder.find(item => item.id === id);
    if (item) {
        item.quantity += delta;
        if (item.quantity <= 0) {
            removeFromOrder(id);
        } else {
            renderOrder();
        }
    }
}

// Render order
function renderOrder() {
    const orderItems = document.getElementById('orderItems');

    if (currentOrder.length === 0) {
        orderItems.innerHTML = `
            <div class="empty-order">
                <i class="fas fa-shopping-basket"></i>
                <p>Chưa có món nào</p>
            </div>
        `;
    } else {
        orderItems.innerHTML = currentOrder.map(item => `
            <div class="order-item">
                <div class="order-item-info">
                    <div class="order-item-name">${item.name}</div>
                    <div class="order-item-price">${item.price.toLocaleString('vi-VN')}₫</div>
                </div>
                <div class="quantity-controls">
                    <button class="qty-btn" onclick="updateQuantity(${item.id}, -1)">-</button>
                    <span class="qty-display">${item.quantity}</span>
                    <button class="qty-btn" onclick="updateQuantity(${item.id}, 1)">+</button>
                </div>
                <button class="btn-remove-item" onclick="removeFromOrder(${item.id})">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `).join('');
    }

    updateSummary();
}

// Update summary
function updateSummary() {
    const subtotal = currentOrder.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const discount = 0;
    const total = subtotal - discount;

    document.getElementById('subtotal').textContent = subtotal.toLocaleString('vi-VN') + '₫';
    document.getElementById('discount').textContent = discount.toLocaleString('vi-VN') + '₫';
    document.getElementById('total').textContent = total.toLocaleString('vi-VN') + '₫';
}

// Clear order
function clearOrder() {
    if (currentOrder.length === 0) {
        showNotification('Đơn hàng đang trống! ', 'info');
        return;
    }

    if (confirm('Xóa tất cả món trong đơn hàng?')) {
        currentOrder = [];
        renderOrder();
        showNotification('Đã xóa đơn hàng', 'info');
    }
}

// Select payment method
function selectPayment(method) {
    currentPaymentMethod = method;

    // ✅ Xóa active khỏi TẤT CẢ các nút
    const buttons = document.querySelectorAll('.payment-btn');
    buttons.forEach(btn => {
        btn.classList.remove('active');
    });

    // ✅ Thêm active vào nút được click
    event.target.closest('.payment-btn').classList.add('active');

    console.log('Selected payment:', method); // Debug
}

// Hold order
function holdOrder() {
    if (currentOrder.length === 0) {
        showNotification('Chưa có món nào trong đơn hàng! ', 'error');
        return;
    }

    showNotification('Đã giữ đơn hàng! ', 'info');
    // Có thể lưu vào localStorage hoặc database
    localStorage.setItem('heldOrder', JSON.stringify({
        items: currentOrder,
        customer: {
            name: document.getElementById('customerName').value,
            phone: document.getElementById('customerPhone').value
        },
        payment: currentPaymentMethod,
        time: new Date().toISOString()
    }));
}

// Complete order
function completeOrder() {
    if (currentOrder.length === 0) {
        showNotification('Chưa có món nào trong đơn hàng!', 'error');
        return;
    }

    const customerName = document.getElementById('customerName').value.trim();
    const customerPhone = document.getElementById('customerPhone').value.trim();

    if (!customerName) {
        showNotification('Vui lòng nhập tên khách hàng!', 'error');
        document.getElementById('customerName').focus();
        return;
    }

    const total = currentOrder.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const itemsList = currentOrder.map(item => `${item.quantity}x ${item.name}`).join(', ');

    const confirmMessage = `━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 XÁC NHẬN ĐƠN HÀNG
━━━━━━━━━━━━━━━━━━━━━━━━━━

👤 Khách hàng: ${customerName}
📞 SĐT: ${customerPhone || 'Không có'}

🍽️ Món ăn:
${currentOrder.map(item => `   • ${item.quantity}x ${item.name} - ${(item.price * item.quantity).toLocaleString('vi-VN')}₫`).join('\n')}

💰 Tổng tiền: ${total.toLocaleString('vi-VN')}₫
💳 Thanh toán: ${getPaymentMethodName(currentPaymentMethod)}

━━━━━━━━━━━━━━━━━━━━━━━━━━
Xác nhận tạo đơn hàng? `;

    if (confirm(confirmMessage)) {
        // Có thể gọi API để lưu đơn hàng vào database
        showNotification('✅ Đơn hàng đã hoàn tất!', 'success');

        // In hóa đơn (có thể kết nối máy in)
        printReceipt(customerName, customerPhone, currentOrder, total, currentPaymentMethod);

        // Reset đơn hàng
        currentOrder = [];
        document.getElementById('customerName').value = '';
        document.getElementById('customerPhone').value = '';
        renderOrder();
    }
}

// Print receipt
function printReceipt(customerName, customerPhone, items, total, payment) {
    const now = new Date();
    const dateStr = now.toLocaleDateString('vi-VN');
    const timeStr = now.toLocaleTimeString('vi-VN');

    const receipt = `
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        🍽️ FOODORDER 🍽️
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Ngày: ${dateStr}          Giờ: ${timeStr}
Nhân viên: ${document.querySelector('. staff-info span').textContent}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Khách hàng: ${customerName}
SĐT: ${customerPhone || 'N/A'}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
MÓN ĂN:
${items.map(item => `${item.quantity}x ${item.name.padEnd(25)} ${(item.price * item.quantity).toLocaleString('vi-VN')}₫`).join('\n')}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

TỔNG CỘNG:            ${total.toLocaleString('vi-VN')}₫
Thanh toán:           ${getPaymentMethodName(payment)}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    CẢM ƠN QUÝ KHÁCH! 
    HẸN GẶP LẠI!  ❤️
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
`;

    console.log(receipt);
    // Có thể mở cửa sổ in hoặc gửi đến máy in
    alert(receipt);
}

// Get payment method name
function getPaymentMethodName(method) {
    const names = {
        'cash': 'Tiền mặt 💵',
        'card': 'Thẻ 💳',
        'momo': 'MoMo 📱'
    };
    return names[method] || 'Tiền mặt';
}

// Show notification
function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 20px;
        background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'};
        color: white;
        padding: 15px 25px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 9999;
        animation: slideIn 0.3s ease;
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(400px); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(400px); opacity: 0; }
    }
`;
document.head.appendChild(style);

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    renderOrder();
});