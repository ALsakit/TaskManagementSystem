/**
 * ملف JavaScript محسن لإدارة واجهة الموظفين
 * يدعم العمليات عبر AJAX مع معالجة شاملة للأخطاء
 */

class EmployeeManager {
    constructor() {
        this.modal = null;
        this.isEditMode = false;
        this.currentEmployeeId = null;
        this.init();
    }

    init() {
        console.log('تهيئة مدير الموظفين');
        this.setupEventListeners();
        this.setupModal();
        this.setupSearch();
        this.updateStats();
    }

    setupEventListeners() {
        // زر إضافة موظف جديد
        const addBtn = document.getElementById('addEmployeeBtn');
        if (addBtn) {
            addBtn.addEventListener('click', () => this.openAddModal());
        }

        // أزرار التعديل
        document.addEventListener('click', (e) => {
            if (e.target.closest('.action-btn.edit')) {
                const employeeId = e.target.closest('.action-btn.edit').dataset.id;
                this.openEditModal(employeeId);
            }
        });

        // أزرار الحذف
        document.addEventListener('click', (e) => {
            if (e.target.closest('.action-btn.delete')) {
                const employeeId = e.target.closest('.action-btn.delete').dataset.id;
                this.deleteEmployee(employeeId);
            }
        });

        // أزرار العرض
        document.addEventListener('click', (e) => {
            if (e.target.closest('.action-btn.view')) {
                const employeeId = e.target.closest('.action-btn.view').dataset.id;
                this.viewEmployee(employeeId);
            }
        });

        // اختصارات لوحة المفاتيح
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'n') {
                e.preventDefault();
                this.openAddModal();
            }
            if (e.key === 'Escape' && this.modal && this.modal.classList.contains('active')) {
                this.closeModal();
            }
        });
    }

    setupModal() {
        // إنشاء النموذج المنبثق إذا لم يكن موجوداً
        if (!document.getElementById('employeeModal')) {
            const modalHTML = `
                <div id="employeeModal" class="modal-overlay">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h3 class="modal-title" id="modalTitle">إضافة موظف جديد</h3>
                            <button type="button" class="modal-close" onclick="employeeManager.closeModal()">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <form id="employeeForm">
                            <div class="form-group">
                                <label class="form-label" for="employeeName">الاسم *</label>
                                <input type="text" id="employeeName" name="Name" class="form-input" required>
                                <div class="error-message" id="nameError"></div>
                            </div>
                            <div class="form-group">
                                <label class="form-label" for="employeeEmail">البريد الإلكتروني *</label>
                                <input type="email" id="employeeEmail" name="Email" class="form-input" required>
                                <div class="error-message" id="emailError"></div>
                            </div>
                            <div class="form-group" id="passwordGroup">
                                <label class="form-label" for="employeePassword">كلمة المرور *</label>
                                <input type="password" id="employeePassword" name="Password" class="form-input" required>
                                <div class="error-message" id="passwordError"></div>
                            </div>
                            <div class="form-group">
                                <label class="form-label" for="employeeRole">الدور *</label>
                                <select id="employeeRole" name="Role" class="form-select" required>
                                    <option value="">اختر الدور</option>
                                    <option value="Admin">مدير</option>
                                    <option value="Manager">مشرف</option>
                                    <option value="Employee">موظف</option>
                                </select>
                                <div class="error-message" id="roleError"></div>
                            </div>
                            <div class="form-actions">
                                <button type="button" class="btn-secondary" onclick="employeeManager.closeModal()">
                                    إلغاء
                                </button>
                                <button type="submit" class="btn-primary" id="submitBtn">
                                    <i class="fas fa-save"></i>
                                    <span id="submitText">حفظ</span>
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            `;
            document.body.insertAdjacentHTML('beforeend', modalHTML);
        }

        this.modal = document.getElementById('employeeModal');
        const form = document.getElementById('employeeForm');

        if (form) {
            form.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }

        // إغلاق النموذج عند النقر خارجه
        this.modal.addEventListener('click', (e) => {
            if (e.target === this.modal) {
                this.closeModal();
            }
        });
    }

    setupSearch() {
        const searchInput = document.getElementById('searchInput');
        const roleFilter = document.getElementById('roleFilter');

        if (searchInput) {
            searchInput.addEventListener('input', () => this.filterEmployees());
        }

        if (roleFilter) {
            roleFilter.addEventListener('change', () => this.filterEmployees());
        }
    }

    openAddModal() {
        console.log('فتح نموذج إضافة موظف جديد');
        this.isEditMode = false;
        this.currentEmployeeId = null;

        document.getElementById('modalTitle').textContent = 'إضافة موظف جديد';
        document.getElementById('submitText').textContent = 'إضافة';
        document.getElementById('passwordGroup').style.display = 'block';
        document.getElementById('employeePassword').required = true;

        this.clearForm();
        this.clearErrors();
        this.showModal();
    }

    async openEditModal(employeeId) {
        console.log('فتح نموذج تعديل الموظف:', employeeId);
        this.isEditMode = true;
        this.currentEmployeeId = employeeId;

        document.getElementById('modalTitle').textContent = 'تعديل بيانات الموظف';
        document.getElementById('submitText').textContent = 'حفظ التعديلات';
        document.getElementById('passwordGroup').style.display = 'none';
        document.getElementById('employeePassword').required = false;

        this.clearForm();
        this.clearErrors();

        try {
            await this.loadEmployeeData(employeeId);
            this.showModal();
        } catch (error) {
            console.error('خطأ في تحميل بيانات الموظف:', error);
            this.showToast('خطأ', 'فشل في تحميل بيانات الموظف', 'error');
        }
    }

    async loadEmployeeData(employeeId) {
        console.log('تحميل بيانات الموظف:', employeeId);

        try {
            const response = await fetch(`/Employee/GetEmployee/${employeeId}`, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            console.log('استجابة تحميل البيانات:', response.status);

            if (response.ok) {
                const result = await response.json();
                console.log('نتيجة تحميل البيانات:', result);

                if (result.success) {
                    const employee = result.data;
                    document.getElementById('employeeName').value = employee.name || '';
                    document.getElementById('employeeEmail').value = employee.email || '';
                    document.getElementById('employeeRole').value = employee.role || '';

                    console.log('تم تحميل البيانات بنجاح');
                } else {
                    throw new Error(result.message || 'فشل في تحميل البيانات');
                }
            } else {
                const errorText = await response.text();
                console.error('خطأ في الاستجابة:', response.status, errorText);
                throw new Error(`خطأ في الخادم (${response.status})`);
            }
        } catch (error) {
            console.error('خطأ في تحميل بيانات الموظف:', error);
            throw error;
        }
    }

    async handleFormSubmit(e) {
        e.preventDefault();
        console.log('بدء معالجة إرسال النموذج');
        console.log('نوع العملية:', this.isEditMode ? 'تعديل' : 'إضافة');

        this.clearErrors();

        // التحقق من صحة البيانات
        if (!this.validateForm()) {
            console.log('فشل التحقق من صحة البيانات');
            return;
        }

        const submitBtn = document.getElementById('submitBtn');
        const originalText = document.getElementById('submitText').textContent;

        // تعطيل الزر وإظهار مؤشر التحميل
        submitBtn.disabled = true;
        document.getElementById('submitText').textContent = 'جاري الحفظ...';

        try {
            const formData = new FormData();
            const name = document.getElementById('employeeName').value.trim();
            const email = document.getElementById('employeeEmail').value.trim();
            const role = document.getElementById('employeeRole').value;

            formData.append('Name', name);
            formData.append('Email', email);
            formData.append('Role', role);

            // إضافة كلمة المرور فقط في حالة الإضافة
            if (!this.isEditMode) {
                const password = document.getElementById('employeePassword').value;
                formData.append('Password', password);
            }

            // إضافة Anti-Forgery Token
            const token = this.getAntiForgeryToken();
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }

            console.log('البيانات المرسلة:');
            for (let [key, value] of formData.entries()) {
                if (key !== '__RequestVerificationToken') {
                    console.log(`${key}: ${value}`);
                }
            }

            const url = this.isEditMode
                ? `/Employee/EditAjax/${this.currentEmployeeId}`
                : '/Employee/CreateAjax';

            console.log('إرسال البيانات إلى:', url);

            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            console.log('استجابة الخادم:', response.status);

            if (response.ok) {
                const result = await response.json();
                console.log('نتيجة العملية:', result);

                if (result.success) {
                    this.showToast('نجح!', result.message, 'success');
                    this.closeModal();

                    // إعادة تحميل الصفحة بعد تأخير قصير
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    if (result.errors) {
                        this.displayErrors(result.errors);
                    } else {
                        this.showToast('خطأ', result.message, 'error');
                    }
                }
            } else {
                const errorText = await response.text();
                console.error('خطأ في الاستجابة:', response.status, errorText);
                this.showToast('خطأ', `خطأ في الخادم (${response.status})`, 'error');
            }
        } catch (error) {
            console.error('خطأ في إرسال البيانات:', error);
            this.showToast('خطأ', 'حدث خطأ أثناء الحفظ', 'error');
        } finally {
            // إعادة تفعيل الزر
            submitBtn.disabled = false;
            document.getElementById('submitText').textContent = originalText;
        }
    }

    async deleteEmployee(employeeId) {
        console.log('طلب حذف الموظف:', employeeId);

        const result = await Swal.fire({
            title: 'تأكيد الحذف',
            text: 'هل أنت متأكد من حذف هذا الموظف؟',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'نعم، احذف',
            cancelButtonText: 'إلغاء',
            reverseButtons: true
        });

        if (result.isConfirmed) {
            try {
                const formData = new FormData();
                const token = this.getAntiForgeryToken();
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }

                const response = await fetch(`/Employee/DeleteConfirmed/${employeeId}`, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        this.showToast('تم الحذف', result.message, 'success');

                        // إزالة الصف مع تأثير بصري
                        const row = document.querySelector(`[data-employee-id="${employeeId}"]`);
                        if (row) {
                            row.style.transition = 'all 0.3s ease';
                            row.style.opacity = '0';
                            row.style.transform = 'translateX(-20px)';

                            setTimeout(() => {
                                row.remove();
                                this.updateStats();
                            }, 300);
                        }
                    } else {
                        this.showToast('خطأ', result.message, 'error');
                    }
                } else {
                    const errorText = await response.text();
                    console.error('خطأ في حذف الموظف:', response.status, errorText);
                    this.showToast('خطأ', `خطأ في الخادم (${response.status})`, 'error');
                }
            } catch (error) {
                console.error('خطأ في حذف الموظف:', error);
                this.showToast('خطأ', 'حدث خطأ أثناء الحذف', 'error');
            }
        }
    }

    async viewEmployee(employeeId) {
        console.log('عرض تفاصيل الموظف:', employeeId);

        try {
            const response = await fetch(`/Employee/GetEmployee/${employeeId}`);

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    const employee = result.data;

                    Swal.fire({
                        title: 'تفاصيل الموظف',
                        html: `
                            <div style="text-align: right; padding: 1rem;">
                                <p><strong>الاسم:</strong> ${employee.name}</p>
                                <p><strong>البريد الإلكتروني:</strong> ${employee.email}</p>
                                <p><strong>الدور:</strong> ${this.getRoleDisplayName(employee.role)}</p>
                            </div>
                        `,
                        icon: 'info',
                        confirmButtonText: 'إغلاق'
                    });
                } else {
                    this.showToast('خطأ', result.message, 'error');
                }
            } else {
                this.showToast('خطأ', 'فشل في جلب تفاصيل الموظف', 'error');
            }
        } catch (error) {
            console.error('خطأ في عرض تفاصيل الموظف:', error);
            this.showToast('خطأ', 'حدث خطأ أثناء جلب التفاصيل', 'error');
        }
    }

    validateForm() {
        let isValid = true;

        const name = document.getElementById('employeeName').value.trim();
        const email = document.getElementById('employeeEmail').value.trim();
        const role = document.getElementById('employeeRole').value;
        const password = document.getElementById('employeePassword').value;

        // التحقق من الاسم
        if (!name) {
            this.showFieldError('nameError', 'الاسم مطلوب');
            isValid = false;
        } else if (name.length < 2) {
            this.showFieldError('nameError', 'الاسم يجب أن يكون أكثر من حرفين');
            isValid = false;
        }

        // التحقق من البريد الإلكتروني
        if (!email) {
            this.showFieldError('emailError', 'البريد الإلكتروني مطلوب');
            isValid = false;
        } else if (!this.isValidEmail(email)) {
            this.showFieldError('emailError', 'البريد الإلكتروني غير صحيح');
            isValid = false;
        }

        // التحقق من كلمة المرور (فقط في حالة الإضافة)
        if (!this.isEditMode) {
            if (!password) {
                this.showFieldError('passwordError', 'كلمة المرور مطلوبة');
                isValid = false;
            } else if (password.length < 6) {
                this.showFieldError('passwordError', 'كلمة المرور يجب أن تكون 6 أحرف على الأقل');
                isValid = false;
            }
        }

        // التحقق من الدور
        if (!role) {
            this.showFieldError('roleError', 'الدور مطلوب');
            isValid = false;
        }

        return isValid;
    }

    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    showFieldError(fieldId, message) {
        const errorElement = document.getElementById(fieldId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        }
    }

    clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(element => {
            element.textContent = '';
            element.style.display = 'none';
        });
    }

    displayErrors(errors) {
        console.log('عرض الأخطاء:', errors);

        for (const [field, messages] of Object.entries(errors)) {
            const errorId = field.toLowerCase() + 'Error';
            const errorElement = document.getElementById(errorId);

            if (errorElement && messages.length > 0) {
                errorElement.textContent = messages[0];
                errorElement.style.display = 'block';
            }
        }
    }

    clearForm() {
        document.getElementById('employeeName').value = '';
        document.getElementById('employeeEmail').value = '';
        document.getElementById('employeePassword').value = '';
        document.getElementById('employeeRole').value = '';
    }

    showModal() {
        if (this.modal) {
            this.modal.classList.add('active');
            document.body.style.overflow = 'hidden';

            // التركيز على أول حقل
            setTimeout(() => {
                document.getElementById('employeeName').focus();
            }, 300);
        }
    }

    closeModal() {
        if (this.modal) {
            this.modal.classList.remove('active');
            document.body.style.overflow = '';
            this.clearForm();
            this.clearErrors();
        }
    }

    filterEmployees() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
        const roleFilter = document.getElementById('roleFilter').value;
        const rows = document.querySelectorAll('.employee-row');

        rows.forEach(row => {
            const name = row.querySelector('.employee-name').textContent.toLowerCase();
            const email = row.querySelector('.employee-email').textContent.toLowerCase();
            const role = row.querySelector('.role-badge').textContent.trim();

            const matchesSearch = name.includes(searchTerm) || email.includes(searchTerm);
            const matchesRole = !roleFilter || role.includes(this.getRoleDisplayName(roleFilter));

            if (matchesSearch && matchesRole) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    updateStats() {
        const rows = document.querySelectorAll('.employee-row');
        const totalEmployees = rows.length;

        document.getElementById('totalEmployees').textContent = totalEmployees;

        // يمكن إضافة منطق لحساب النشطين وغير النشطين إذا كانت البيانات متوفرة
        // document.getElementById('activeEmployees').textContent = activeCount;
        // document.getElementById('inactiveEmployees').textContent = inactiveCount;
    }

    getRoleDisplayName(role) {
        const roleNames = {
            'Admin': 'مدير',
            'Manager': 'مشرف',
            'Employee': 'موظف'
        };
        return roleNames[role] || role;
    }

    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : null;
    }

    showToast(title, message, type = 'info') {
        const icon = type === 'success' ? 'success' : type === 'error' ? 'error' : 'info';

        Swal.fire({
            title: title,
            text: message,
            icon: icon,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true
        });
    }
}

// تهيئة مدير الموظفين عند تحميل الصفحة
let employeeManager;

document.addEventListener('DOMContentLoaded', function () {
    console.log('تحميل الصفحة مكتمل، تهيئة مدير الموظفين');
    employeeManager = new EmployeeManager();
});

// تصدير للاستخدام العام
window.employeeManager = employeeManager;

