// Charity Donation Platform - Main JavaScript File

document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            if (alert && alert.parentNode) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('href').substring(1);
            const targetElement = document.getElementById(targetId);
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Form validation enhancement
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Number formatting for currency inputs
    const currencyInputs = document.querySelectorAll('input[type="number"][data-currency]');
    currencyInputs.forEach(function (input) {
        input.addEventListener('blur', function () {
            const value = parseFloat(this.value);
            if (!isNaN(value)) {
                this.value = value.toFixed(2);
            }
        });
    });

    // Progress bar animations
    const progressBars = document.querySelectorAll('.progress-bar');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const progressObserver = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                const progressBar = entry.target;
                const width = progressBar.style.width || progressBar.getAttribute('aria-valuenow') + '%';
                progressBar.style.width = '0%';
                setTimeout(function () {
                    progressBar.style.transition = 'width 1s ease-in-out';
                    progressBar.style.width = width;
                }, 100);
                progressObserver.unobserve(progressBar);
            }
        });
    }, observerOptions);

    progressBars.forEach(function (bar) {
        progressObserver.observe(bar);
    });

    // Image lazy loading
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(function (img) {
        imageObserver.observe(img);
    });

    // Search functionality
    const searchInputs = document.querySelectorAll('input[data-search]');
    searchInputs.forEach(function (input) {
        input.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            const targetSelector = this.dataset.search;
            const targets = document.querySelectorAll(targetSelector);

            targets.forEach(function (target) {
                const text = target.textContent.toLowerCase();
                const shouldShow = text.includes(searchTerm);
                target.style.display = shouldShow ? '' : 'none';
            });
        });
    });

    // Copy to clipboard functionality
    const copyButtons = document.querySelectorAll('[data-copy]');
    copyButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            const textToCopy = this.dataset.copy;
            navigator.clipboard.writeText(textToCopy).then(function () {
                const originalText = button.textContent;
                button.textContent = 'Copied!';
                button.classList.add('btn-success');
                setTimeout(function () {
                    button.textContent = originalText;
                    button.classList.remove('btn-success');
                }, 2000);
            });
        });
    });

    // Loading states for buttons
    const loadingButtons = document.querySelectorAll('[data-loading]');
    loadingButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            const originalText = this.innerHTML;
            this.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Loading...';
            this.disabled = true;

            // Re-enable after form submission or 10 seconds
            setTimeout(function () {
                button.innerHTML = originalText;
                button.disabled = false;
            }, 10000);
        });
    });
});

// Utility Functions
window.CharityPlatform = {
    // Format currency
    formatCurrency: function (amount, currency = 'USD') {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency
        }).format(amount);
    },

    // Format percentage
    formatPercentage: function (value, decimals = 1) {
        return (value).toFixed(decimals) + '%';
    },

    // Show toast notification
    showToast: function (message, type = 'info') {
        const toastContainer = document.getElementById('toast-container') || this.createToastContainer();
        const toast = this.createToast(message, type);
        toastContainer.appendChild(toast);

        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', function () {
            toast.remove();
        });
    },

    createToastContainer: function () {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1055';
        document.body.appendChild(container);
        return container;
    },

    createToast: function (message, type) {
        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="toast-header">
                <i class="fas fa-${this.getToastIcon(type)} text-${type} me-2"></i>
                <strong class="me-auto">Notification</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">${message}</div>
        `;
        return toast;
    },

    getToastIcon: function (type) {
        const icons = {
            'success': 'check-circle',
            'error': 'exclamation-circle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle'
        };
        return icons[type] || 'info-circle';
    },

    // Debounce function
    debounce: function (func, wait, immediate) {
        let timeout;
        return function executedFunction() {
            const context = this;
            const args = arguments;
            const later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    },

    // Validate email
    isValidEmail: function (email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },

    // Load notifications count with error handling
    loadNotificationCount: function () {
        // Only try to load notifications if user is authenticated
        const notificationBadge = document.getElementById('notification-badge');
        if (!notificationBadge) {
            return; // No notification badge, user probably not logged in
        }

        fetch('/api/Notifications/count')
            .then(function (response) {
                if (!response.ok) {
                    // If API doesn't exist or fails, just hide the badge
                    if (notificationBadge) {
                        notificationBadge.style.display = 'none';
                    }
                    return null;
                }
                return response.json();
            })
            .then(function (data) {
                if (data && notificationBadge) {
                    if (data.count > 0) {
                        notificationBadge.textContent = data.count;
                        notificationBadge.style.display = 'inline';
                    } else {
                        notificationBadge.style.display = 'none';
                    }
                }
            })
            .catch(function (error) {
                // Silently handle errors - API might not be implemented yet
                console.log('Notifications API not available:', error.message);
                if (notificationBadge) {
                    notificationBadge.style.display = 'none';
                }
            });
    }
};

// NOTIFICATION LOADING DISABLED UNTIL API IS IMPLEMENTED
// This prevents 404 errors in the console
/*
// Load notification count on page load if user is authenticated
const navbarDropdown = document.querySelector('.navbar .dropdown-toggle');
if (navbarDropdown && document.getElementById('notification-badge')) {
    CharityPlatform.loadNotificationCount();

    // Refresh notification count every 30 seconds, but only if the API works
    const testInterval = setInterval(function () {
        fetch('/api/Notifications/count')
            .then(function (response) {
                if (response.ok) {
                    CharityPlatform.loadNotificationCount();
                } else {
                    // API not working, stop trying
                    clearInterval(testInterval);
                }
            })
            .catch(function () {
                // API not working, stop trying
                clearInterval(testInterval);
            });
    }, 30000);
}
*/