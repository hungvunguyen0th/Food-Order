// ================================================
// POS SYSTEM - QUẢN LÝ BÁN HÀNG
// ================================================

let currentOrder = {
    items: [],
    customerName: '',
    customerPhone: '',
    paymentMethod: 'cash',
    discount: 0,
    discountCode: null
};

let heldOrders = [];
let availableDiscounts = [];

// ================================================
// KHỞI TẠO HỆ THỐNG
// ================================================
function initPOS() {
    loadFromLocalStorage();
    setupEventListeners();
    fetchAvailableDiscounts();
    updateOrderDisplay();
    renderHeldOrdersList();
    ensureDefaultPaymentButton();
    console.log('POS System initialized');
}

// ================================================
// LOAD DISCOUNTS FROM SERVER
// ================================================
async function fetchAvailableDiscounts() {
    try {
        const res = await fetch('/api/discounts/active');
        if (!res.ok) return;
        availableDiscounts = await res.json();
        const select = document.getElementById('discountSelect');
        if (!select) return;
        select.innerHTML = '<option value="">Chọn mã</option>' + availableDiscounts.map(d => `
            <option value="${d.code}">${d.code} - ${d.discountPercent}%</option>`).join('');
    } catch (err) {
        console.error('Không thể tải mã giảm giá', err);
    }
}

function applyDiscountFromInput() {
    const input = document.getElementById('discountCodeInput');
    const select = document.getElementById('discountSelect');
    const code = (input.value || select.value || '').trim().toUpperCase();
    if (!code) {
        showToast('Nhập mã giảm giá hoặc chọn từ danh sách', 'warning');
        return;
    }

    const discount = availableDiscounts.find(d => d.code.toUpperCase() === code && d.isActive);
    if (!discount) {
        showToast('Mã giảm giá không hợp lệ hoặc đã hết hạn', 'error');
        return;
    }

    const subtotal = currentOrder.items.reduce((s, it) => s + it.total, 0);
    let amount = 0;
    if (discount.discountPercent && discount.discountPercent > 0) {
        amount = Math.floor(subtotal * (discount.discountPercent / 100));
    } else if (discount.maxDiscountAmount) {
        amount = Math.min(discount.maxDiscountAmount, subtotal);
    }

    currentOrder.discount = amount;
    currentOrder.discountCode = discount.code;

    document.getElementById('appliedDiscount').style.display = 'block';
    document.getElementById('appliedDiscountText').textContent = `${discount.code} (-${formatCurrency(amount)})`;
    updateSummary();
    saveToLocalStorage();
    showToast('Áp dụng mã giảm giá thành công', 'success');
}

function clearAppliedDiscount() {
    currentOrder.discount = 0;
    currentOrder.discountCode = null;
    document.getElementById('appliedDiscount').style.display = 'none';
    document.getElementById('appliedDiscountText').textContent = '';
    updateSummary();
    saveToLocalStorage();
}

// ================================================
// LOCALSTORAGE - LƯU TRỮ DỮ LIỆU
// ================================================
function loadFromLocalStorage() {
    try {
        const savedOrder = localStorage.getItem('currentOrder');
        const savedHeldOrders = localStorage.getItem('heldOrders');

        if (savedOrder) {
            currentOrder = JSON.parse(savedOrder);
            if (currentOrder.customerName) document.getElementById('customerName').value = currentOrder.customerName;
            if (currentOrder.customerPhone) document.getElementById('customerPhone').value = currentOrder.customerPhone;
        }

        if (savedHeldOrders) {
            heldOrders = JSON.parse(savedHeldOrders);
        }
    } catch (error) {
        console.error('Error loading from localStorage:', error);
    }
}

function saveToLocalStorage() {
    try {
        currentOrder.customerName = document.getElementById('customerName').value;
        currentOrder.customerPhone = document.getElementById('customerPhone').value;
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
    const existingItem = currentOrder.items.find(item => item.productId === productId && item.name === productName);
    if (existingItem) {
        existingItem.quantity++;
        existingItem.total = existingItem.quantity * existingItem.price;
        showToast(`Đã tăng số lượng ${productName}`, 'success');
    } else {
        currentOrder.items.push({ productId, name: productName, price, quantity: 1, total: price });
        showToast(`Đã thêm ${productName}`, 'success');
    }
    updateOrderDisplay();
    saveToLocalStorage();
}

function increaseQuantity(productId) {
    const item = currentOrder.items.find(i => i.productId === productId);
    if (item) { item.quantity++; item.total = item.quantity * item.price; updateOrderDisplay(); saveToLocalStorage(); }
}

function decreaseQuantity(productId) {
    const item = currentOrder.items.find(i => i.productId === productId);
    if (!item) return;
    if (item.quantity > 1) { item.quantity--; item.total = item.quantity * item.price; }
    else { if (!confirm(`Xóa "${item.name}" khỏi đơn hàng?`)) return; removeItem(productId); }
    updateOrderDisplay(); saveToLocalStorage();
}

function removeItem(productId) { const item = currentOrder.items.find(i => i.productId === productId); currentOrder.items = currentOrder.items.filter(i => i.productId !== productId); updateOrderDisplay(); saveToLocalStorage(); if (item) showToast(`Đã xóa ${item.name}`, 'info'); }

function clearOrder() {
    if (!currentOrder.items.length) { showToast('Đơn hàng đang trống', 'warning'); return; }
    if (!confirm('Bạn có chắc muốn xóa toàn bộ đơn hàng?')) return;
    currentOrder = { items: [], customerName: '', customerPhone: '', paymentMethod: 'cash', discount: 0, discountCode: null };
    document.getElementById('customerName').value = '';
    document.getElementById('customerPhone').value = '';
    clearAppliedDiscount();
    updateOrderDisplay(); saveToLocalStorage(); showToast('Đã xóa đơn hàng', 'info');
}

// ================================================
// HELD ORDERS
// ================================================
function holdOrder() {
    if (!currentOrder.items.length) { showToast('Chưa có món nào để giữ', 'warning'); return; }
    const name = document.getElementById('customerName').value || 'Khách vãng lai';
    const snapshot = JSON.parse(JSON.stringify(currentOrder));
    snapshot.customerName = name;
    snapshot.heldAt = new Date().toISOString();
    heldOrders.push(snapshot);
    currentOrder = { items: [], customerName: '', customerPhone: '', paymentMethod: 'cash', discount: 0, discountCode: null };
    document.getElementById('customerName').value = '';
    document.getElementById('customerPhone').value = '';
    clearAppliedDiscount();
    saveToLocalStorage(); renderHeldOrdersList(); updateOrderDisplay(); showToast(`Đã giữ đơn hàng của ${name}`, 'success');
}

function renderHeldOrdersList() {
    const list = document.getElementById('heldOrdersList');
    const count = document.getElementById('heldCount');
    list.innerHTML = '';
    if (!heldOrders.length) { list.innerHTML = '<div style="color:#999;padding:8px">Không có đơn giữ</div>'; count.textContent = '0'; return; }
    heldOrders.forEach((h, idx) => {
        const el = document.createElement('div');
        el.className = 'held-order-item';
        el.style.padding = '8px';
        el.style.borderBottom = '1px solid #eee';
        el.innerHTML = `<div style="display:flex;justify-content:space-between;align-items:center;">
            <div style="flex:1;min-width:0">
                <div style="font-weight:700;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${h.customerName} <small style="color:#666">(${new Date(h.heldAt).toLocaleTimeString()})</small></div>
                <div style="font-size:12px;color:#666;overflow:hidden;text-overflow:ellipsis">${h.items.map(it=>`${it.name} x${it.quantity}`).join(', ')}</div>
            </div>
            <div style="display:flex;gap:6px;margin-left:8px">
                <button class="btn-apply-discount" onclick="restoreHeldOrder(${idx})">Phục hồi</button>
                <button class="btn-apply-discount" onclick="deleteHeldOrder(${idx})">Xóa</button>
            </div>
        </div>`;
        list.appendChild(el);
    });
    count.textContent = heldOrders.length.toString();
}

function restoreHeldOrder(index) {
    const h = heldOrders[index];
    if (!h) return;
    currentOrder = JSON.parse(JSON.stringify(h));
    heldOrders.splice(index, 1);
    saveToLocalStorage(); renderHeldOrdersList(); updateOrderDisplay(); showToast('Đã phục hồi đơn giữ', 'success');
}

function deleteHeldOrder(index) { if (!confirm('Xác nhận xóa đơn giữ?')) return; heldOrders.splice(index,1); saveToLocalStorage(); renderHeldOrdersList(); showToast('Đã xóa đơn giữ', 'info'); }
function clearAllHeldOrders(){ if(!heldOrders.length){showToast('Không có đơn giữ', 'warning'); return;} if(!confirm('Xóa tất cả đơn giữ?')) return; heldOrders=[]; saveToLocalStorage(); renderHeldOrdersList(); showToast('Đã xóa tất cả đơn giữ','info'); }

// ================================================
// CẬP NHẬT HIỂN THỊ
// ================================================
function updateOrderDisplay() {
    const orderItemsContainer = document.getElementById('orderItems');
    if (!orderItemsContainer) return;
    if (!currentOrder.items.length) {
        orderItemsContainer.innerHTML = `\n            <div class="empty-order">\n                <i class="fas fa-shopping-basket"></i>\n                <p>Chưa có món nào</p>\n            </div>\n        `;
    } else {
        orderItemsContainer.innerHTML = currentOrder.items.map(item => `\n            <div class="order-item" data-id="${item.productId}">\n                <div class="item-info">\n                    <h4 class="order-item-name">${item.name}</h4>\n                    <p class="item-price">${formatCurrency(item.price)}</p>\n                </div>\n                <div class="item-controls">\n                    <button onclick="decreaseQuantity(${item.productId})" class="btn-quantity">-</button>\n                    <div class="quantity">${item.quantity}</div>\n                    <button onclick="increaseQuantity(${item.productId})" class="btn-quantity">+</button>\n                    <button onclick="removeItem(${item.productId})" class="btn-remove" title="Xóa món">🗑</button>\n                </div>\n                <div class="item-total">${formatCurrency(item.total)}</div>\n            </div>\n        `).join('');
    }
    updateSummary();
}

function updateSummary() {
    const subtotal = currentOrder.items.reduce((sum, item) => sum + item.total, 0);
    const discount = currentOrder.discount || 0;
    const total = Math.max(0, subtotal - discount);
    const subEl = document.getElementById('subtotal'); if(subEl) subEl.textContent = formatCurrency(subtotal);
    const discEl = document.getElementById('discount'); if(discEl) discEl.textContent = formatCurrency(discount);
    const totEl = document.getElementById('total'); if(totEl) totEl.textContent = formatCurrency(total);
}

// ================================================
// SEARCH & FILTER FIXES
// ================================================
function filterCategory(category) {
    const products = document.querySelectorAll('.product-card');
    document.querySelectorAll('.category-btn').forEach(btn=>btn.classList.remove('active'));
    // find clicked button by text
    document.querySelectorAll('.category-btn').forEach(btn=>{ if(btn.textContent.trim()===category || category==='all' && btn.textContent.includes('Tất cả')) btn.classList.add('active'); });
    products.forEach(product=>{ if(category==='all' || product.dataset.category===category) product.style.display='block'; else product.style.display='none'; });
}

let searchTimeout;
function searchProducts(){ clearTimeout(searchTimeout); searchTimeout=setTimeout(()=>{ const term=(document.getElementById('searchProduct').value||'').toLowerCase(); document.querySelectorAll('.product-card').forEach(p=>{ const name=p.dataset.name.toLowerCase(); p.style.display = name.includes(term) ? 'block' : 'none'; }); },300); }

// ================================================
// THANH TOÁN
// ================================================
function selectPayment(method) {
    currentOrder.paymentMethod = method;

    const buttons = document.querySelectorAll('.payment-btn');
    buttons.forEach(btn => btn.classList.remove('active'));

    const matched = document.querySelectorAll(`.payment-btn[data-method="${method}"]`);
    matched.forEach(b => {
        b.classList.add('active');
        // small pulse animation for feedback
        b.classList.add('selected-pulse');
        setTimeout(() => b.classList.remove('selected-pulse'), 300);
    });

    saveToLocalStorage();
}

// Ensure default active payment button is set on load
function ensureDefaultPaymentButton() {
    const active = document.querySelector('.payment-btn.active');
    if (!active) {
        const cashBtn = document.querySelector('.payment-btn[data-method="cash"]');
        if (cashBtn) cashBtn.classList.add('active');
    }
}