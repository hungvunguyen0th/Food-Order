// ================================================
// POS SYSTEM - QUẢN LÝ BÁN HÀNG
// ================================================

let currentOrder = {
    items: [],
    customerName: '',
    customerPhone: '',
    paymentMethod: 'cash',
    discount: 0
};

let heldOrders = [];

// ================================================
// KHỞI TẠO HỆ THỐNG
// ================================================
function initPOS() {
    loadFromLocalStorage();
    setupEventListeners();
    updateOrderDisplay();
    console.log('POS System initialized');
}

// ================================================
// LOCALSTORAGE - LƯU TRỮ DỮ LIỆU
// ================================================
function loadFromLocalStorage() {
    try {
        const savedOrder = localStorage.getItem('currentOrder');
        const savedHeldOrders = localStorage. getItem('heldOrders');
        
        if (savedOrder) {
            currentOrder = JSON.parse(savedOrder);
            // Restore customer info
            if (currentOrder.customerName) {
                document.getElementById('customerName').value = currentOrder.customerName;
            }
            if (currentOrder.customerPhone) {
                document.getElementById('customerPhone'). value = currentOrder.customerPhone;
            }
            updateOrderDisplay();
        }
        
        if (savedHeldOrders) {
            heldOrders = JSON.parse(savedHeldOrders);
            updateHeldOrdersBadge();
        }
    } catch (error) {
        console.error('Error loading from localStorage:', error);
    }
}

function saveToLocalStorage() {
    try {
        // Save customer info
        currentOrder.customerName = document.getElementById('customerName').value;
        currentOrder.customerPhone = document. getElementById('customerPhone').value;
        
        localStorage.setItem('currentOrder', JSON.stringify(currentOrder));
        localStorage.setItem('heldOrders', JSON.stringify(heldOrders));
    } catch (error) {
        console.error('Error saving to localStorage:', error);
    }
}

// ================================================
// QUẢN LÝ ĐƠN HÀNG
// ================================================
function addToOrder(productId, productName, price) {
    const existingItem = currentOrder.items.find(item => item.productId === productId);
    
    if (existingItem) {
        existingItem.quantity++;
        existingItem.total = existingItem.quantity * existingItem.price;
        showToast(`Đã tăng số lượng ${productName}`, 'success');
    } else {
        currentOrder.items.push({
            productId: productId,
            name: productName,
            price: price,
            quantity: 1,
            total: price
        });
        showToast(`Đã thêm ${productName}`, 'success');
    }
    
    updateOrderDisplay();
    saveToLocalStorage();
}

function increaseQuantity(productId) {
    const item = currentOrder. items.find(i => i.productId === productId);
    if (item) {
        item.quantity++;
        item.total = item.quantity * item.price;
        updateOrderDisplay();
        saveToLocalStorage();
    }
}

function decreaseQuantity(productId) {
    const item = currentOrder.items.find(i => i.productId === productId);
    if (item) {
        if (item.quantity > 1) {
            item.quantity--;
            item.total = item.quantity * item.price;
        } else {
            if (confirm(`Xóa "${item.name}" khỏi đơn hàng?`)) {
                removeItem(productId);
                return;
            } else {
                return;
            }
        }
        updateOrderDisplay();
        saveToLocalStorage();
    }
}

function removeItem(productId) {
    const item = currentOrder.items.find(i => i.productId === productId);
    currentOrder.items = currentOrder.items.filter(i => i.productId !== productId);
    updateOrderDisplay();
    saveToLocalStorage();
    if (item) {
        showToast(`Đã xóa ${item.name}`, 'info');
    }
}

function clearOrder() {
    if (currentOrder.items.length === 0) {
        showToast('Đơn hàng đang trống', 'warning');
        return;
    }
    
    if (confirm('Bạn có chắc muốn xóa toàn bộ đơn hàng?')) {
        currentOrder. items = [];
        currentOrder. discount = 0;
        
        updateOrderDisplay();
        saveToLocalStorage();
        showToast('Đã xóa đơn hàng', 'info');
    }
}

// ================================================
// CẬP NHẬT HIỂN THỊ
// ================================================
function updateOrderDisplay() {
    const orderItemsContainer = document.getElementById('orderItems');
    
    if (currentOrder.items.length === 0) {
        orderItemsContainer.innerHTML = `
            <div class="empty-order">
                <i class="fas fa-shopping-basket"></i>
                <p>Chưa có món nào</p>
            </div>
        `;
    } else {
        orderItemsContainer.innerHTML = currentOrder.items.map(item => `
            <div class="order-item" data-id="${item.productId}">
                <div class="item-info">
                    <h4>${item.name}</h4>
                    <p class="item-price">${formatCurrency(item.price)}</p>
                </div>
                <div class="item-controls">
                    <button onclick="decreaseQuantity(${item.productId})" class="btn-quantity">
                        <i class="fas fa-minus"></i>
                    </button>
                    <span class="quantity">${item.quantity}</span>
                    <button onclick="increaseQuantity(${item.productId})" class="btn-quantity">
                        <i class="fas fa-plus"></i>
                    </button>
                    <button onclick="removeItem(${item.productId})" class="btn-remove" title="Xóa món">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
                <div class="item-total">${formatCurrency(item.total)}</div>
            </div>
        `).join('');
    }
    
    updateSummary();
}

function updateSummary() {
    const subtotal = currentOrder.items.reduce((sum, item) => sum + item.total, 0);
    const discount = currentOrder.discount || 0;
    const total = subtotal - discount;
    
    document.getElementById('subtotal'). textContent = formatCurrency(subtotal);
    document.getElementById('discount').textContent = formatCurrency(discount);
    document.getElementById('total').textContent = formatCurrency(total);
}

function updateHeldOrdersBadge() {
    // Có thể thêm badge vào header để hiển thị số đơn đang giữ
    console.log(`Held orders: ${heldOrders.length}`);
}

// ================================================
// CHỨC NĂNG TÌM KIẾM VÀ LỌC
// ================================================
function filterCategory(category) {
    const products = document.querySelectorAll('.product-card');
    const buttons = document.querySelectorAll('. category-btn');
    
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    
    products.forEach(product => {
        if (category === 'all' || product.dataset.category === category) {
            product.style.display = 'block';
        } else {
            product.style.display = 'none';
        }
    });
}

let searchTimeout;
function searchProducts() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        const searchTerm = document.getElementById('searchProduct'). value.toLowerCase();
        const products = document.querySelectorAll('. product-card');
        
        let foundCount = 0;
        products.forEach(product => {
            const name = product.dataset.name.toLowerCase();
            if (name.includes(searchTerm)) {
                product.style.display = 'block';
                foundCount++;
            } else {
                product.style.display = 'none';
            }
        });
        
        if (searchTerm && foundCount === 0) {
            showToast('Không tìm thấy món nào', 'info');
        }
    }, 300);
}

// ================================================
// THANH TOÁN
// ================================================
function selectPayment(method) {
    currentOrder.paymentMethod = method;
    
    const buttons = document.querySelectorAll('. payment-btn');
    buttons. forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.method === method) {
            btn.classList.add('active');
        }
    });
    
    saveToLocalStorage();
}

function holdOrder() {
    if (currentOrder.items.length === 0) {
        showToast('Chưa có món nào để giữ', 'warning');
        return;
    }
    
    const customerName = document.getElementById('customerName').value || 'Khách vãng lai';
    
    heldOrders.push({
        ...JSON.parse(JSON.stringify(currentOrder)),
        customerName: customerName,
        heldAt: new Date().toISOString()
    });
    
    // Reset current order
    currentOrder = {
        items: [],
        customerName: '',
        customerPhone: '',
        paymentMethod: 'cash',
        discount: 0
    };
    
    document.getElementById('customerName').value = '';
    document.getElementById('customerPhone').value = '';
    
    // Reset payment method
    document.querySelectorAll('.payment-btn'). forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.method === 'cash') {
            btn.classList.add('active');
        }
    });
    
    updateOrderDisplay();
    saveToLocalStorage();
    updateHeldOrdersBadge();
    showToast(`Đã giữ đơn hàng của ${customerName}`, 'success');
}

async function completeOrder() {
    if (currentOrder.items.length === 0) {
        showToast('Chưa có món nào trong đơn', 'warning');
        return;
    }
    
    const customerName = document.getElementById('customerName').value || 'Khách vãng lai';
    const customerPhone = document.getElementById('customerPhone').value || '';
    
    if (!confirm(`Xác nhận hoàn tất đơn hàng cho ${customerName}?`)) {
        return;
    }
    
    const orderData = {
        CustomerName: customerName,
        CustomerPhone: customerPhone,
        PaymentMethod: currentOrder.paymentMethod,
        Note: '',
        Items: currentOrder.items.map(item => ({
            ProductID: item.productId,
            Quantity: item.quantity,
            UnitPrice: item.price
        }))
    };
    
    try {
        showLoading(true);
        
        const response = await fetch('/api/POS/orders', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(orderData)
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showToast('Đơn hàng đã được tạo thành công!', 'success');
            
            // Print receipt
            printReceipt({
                ... result.data,
                items: currentOrder.items
            });
            
            // Reset order
            currentOrder = {
                items: [],
                customerName: '',
                customerPhone: '',
                paymentMethod: 'cash',
                discount: 0
            };
            
            document. getElementById('customerName').value = '';
            document.getElementById('customerPhone').value = '';
            
            // Reset payment
            document.querySelectorAll('. payment-btn').forEach(btn => {
                btn.classList.remove('active');
                if (btn.dataset.method === 'cash') {
                    btn.classList.add('active');
                }
            });
            
            updateOrderDisplay();
            saveToLocalStorage();
        } else {
            showToast('Lỗi: ' + (result.message || 'Không thể tạo đơn hàng'), 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi kết nối: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// ================================================
// IN HÓA ĐƠN
// ================================================
function printReceipt(orderData) {
    const subtotal = currentOrder.items.reduce((sum, item) => sum + item.total, 0);
    const discount = currentOrder.discount || 0;
    const total = subtotal - discount;
    
    const printWindow = window.open('', '_blank');
    const printContent = `
        <! DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="utf-8">
            <title>Hóa đơn #${orderData.orderID || 'TEMP'}</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { 
                    font-family: 'Courier New', monospace; 
                    padding: 20px;
                    max-width: 400px;
                    margin: 0 auto;
                }
                h1 { 
                    text-align: center; 
                    font-size: 24px;
                    margin-bottom: 10px;
                    border-bottom: 2px solid #000;
                    padding-bottom: 10px;
                }
                .info { margin: 15px 0; }
                . info p { margin: 5px 0; font-size: 14px; }
                table { 
                    width: 100%; 
                    border-collapse: collapse; 
                    margin: 20px 0;
                }
                th, td { 
                    padding: 8px; 
                    text-align: left; 
                    border-bottom: 1px dashed #666;
                    font-size: 14px;
                }
                th { font-weight: bold; }
                .text-right { text-align: right; }
                .summary { 
                    margin-top: 20px;
                    border-top: 2px solid #000;
                    padding-top: 10px;
                }
                . summary p {
                    display: flex;
                    justify-content: space-between;
                    margin: 8px 0;
                    font-size: 14px;
                }
                . total { 
                    font-weight: bold; 
                    font-size: 18px;
                    border-top: 1px solid #000;
                    padding-top: 10px;
                    margin-top: 10px;
                }
                . footer {
                    text-align: center;
                    margin-top: 30px;
                    font-size: 12px;
                    border-top: 2px solid #000;
                    padding-top: 15px;
                }
                @media print {
                    body { padding: 0; }
                }
            </style>
        </head>
        <body>
            <h1>HÓA ĐƠN BÁN HÀNG</h1>
            
            <div class="info">
                <p><strong>Mã đơn:</strong> #${orderData.orderID || 'TEMP'}</p>
                <p><strong>Khách hàng:</strong> ${orderData.customerName || 'Khách vãng lai'}</p>
                <p><strong>Điện thoại:</strong> ${orderData.customerPhone || 'N/A'}</p>
                <p><strong>Thời gian:</strong> ${new Date(). toLocaleString('vi-VN')}</p>
                <p><strong>Thu ngân:</strong> ${document.querySelector('. staff-info span')?.textContent || 'Staff'}</p>
            </div>
            
            <table>
                <thead>
                    <tr>
                        <th>Món</th>
                        <th class="text-right">SL</th>
                        <th class="text-right">Đơn giá</th>
                        <th class="text-right">Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    ${currentOrder.items.map(item => `
                        <tr>
                            <td>${item.name}</td>
                            <td class="text-right">${item.quantity}</td>
                            <td class="text-right">${formatCurrency(item.price)}</td>
                            <td class="text-right">${formatCurrency(item.total)}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
            
            <div class="summary">
                <p>
                    <span>Tạm tính:</span>
                    <span>${formatCurrency(subtotal)}</span>
                </p>
                ${discount > 0 ?  `
                <p>
                    <span>Giảm giá:</span>
                    <span>-${formatCurrency(discount)}</span>
                </p>
                ` : ''}
                <p class="total">
                    <span>TỔNG CỘNG:</span>
                    <span>${formatCurrency(total)}</span>
                </p>
                <p>
                    <span>Thanh toán:</span>
                    <span>${getPaymentMethodText(currentOrder. paymentMethod)}</span>
                </p>
            </div>
            
            <div class="footer">
                <p>Cảm ơn quý khách! </p>
                <p>Hẹn gặp lại! </p>
            </div>
            
            <script>
                window.onload = function() {
                    setTimeout(() => {
                        window. print();
                    }, 500);
                }
                window.onafterprint = function() { 
                    window.close(); 
                }
            </script>
        </body>
        </html>
    `;
    
    printWindow.document.write(printContent);
    printWindow.document.close();
}

function getPaymentMethodText(method) {
    const methods = {
        'cash': 'Tiền mặt',
        'card': 'Thẻ',
        'momo': 'MoMo'
    };
    return methods[method] || 'Tiền mặt';
}

// ================================================
// TIỆN ÍCH
// ================================================
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { 
        style: 'currency', 
        currency: 'VND' 
    }).format(amount);
}

function showLoading(show) {
    let overlay = document.getElementById('loadingOverlay');
    if (! overlay) {
        overlay = document.createElement('div');
        overlay.id = 'loadingOverlay';
        overlay.className = 'loading-overlay';
        overlay.innerHTML = `
            <div class="spinner-container">
                <div class="spinner"></div>
                <p>Đang xử lý...</p>
            </div>
        `;
        document.body.appendChild(overlay);
    }
    overlay.style.display = show ? 'flex' : 'none';
}

function showToast(message, type = 'info') {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container';
        document.body. appendChild(container);
    }
    
    const toast = document. createElement('div');
    toast. className = `toast toast-${type}`;
    
    const icons = {
        'success': 'fa-check-circle',
        'error': 'fa-times-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    };
    
    toast.innerHTML = `
        <i class="fas ${icons[type] || icons. info}"></i>
        <span>${message}</span>
    `;
    
    container. appendChild(toast);
    
    setTimeout(() => {
        toast.classList.add('show');
    }, 10);
    
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ================================================
// EVENT LISTENERS
// ================================================
function setupEventListeners() {
    // Double click để thêm nhanh sản phẩm
    document. querySelectorAll('.product-card'). forEach(card => {
        card.addEventListener('dblclick', function() {
            const btn = this.querySelector('.btn-add');
            if (btn) btn.click();
        });
    });
    
    // Keyboard shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl+S - Hoàn tất đơn
        if ((e.ctrlKey || e. metaKey) && e.key === 's') {
            e.preventDefault();
            completeOrder();
        }
        // Ctrl+H - Giữ đơn
        else if ((e.ctrlKey || e.metaKey) && e.key === 'h') {
            e.preventDefault();
            holdOrder();
        }
        // Ctrl+D - Xóa đơn
        else if ((e.ctrlKey || e.metaKey) && e.key === 'd') {
            e.preventDefault();
            clearOrder();
        }
        // ESC - Clear search
        else if (e.key === 'Escape') {
            const searchInput = document.getElementById('searchProduct');
            if (searchInput && searchInput.value) {
                searchInput.value = '';
                searchProducts();
            }
        }
    });
    
    // Save customer info on change
    const customerName = document.getElementById('customerName');
    const customerPhone = document.getElementById('customerPhone');
    
    if (customerName) {
        customerName.addEventListener('change', saveToLocalStorage);
    }
    if (customerPhone) {
        customerPhone.addEventListener('change', saveToLocalStorage);
    }
    
    console.log('Event listeners setup complete');
    console.log('Keyboard shortcuts: Ctrl+S (Complete), Ctrl+H (Hold), Ctrl+D (Clear)');
}

// ================================================
// KHỞI TẠO KHI DOM READY
// ================================================
if (document.readyState === 'loading') {
    document. addEventListener('DOMContentLoaded', initPOS);
} else {
    initPOS();
}