/**
 * Enhanced Theme Toggle Functionality
 * Handles dark/light mode switching with persistence and smooth transitions
 */

class ThemeManager {
    constructor() {
        this.themes = {
            LIGHT: 'light',
            DARK: 'dark'
        };
        
        this.currentTheme = localStorage.getItem('booking-theme') || this.themes.DARK;
        this.toggle = document.getElementById('themeToggle');
        this.html = document.documentElement;
        
        this.init();
    }

    init() {
        // Set initial state
        this.updateTheme(this.currentTheme, false);
        this.updateToggleState();
        
        // Add event listeners
        this.addEventListeners();
        
        // Add transition class after initialization
        setTimeout(() => {
            document.body.classList.add('theme-transition');
        }, 100);
    }

    addEventListeners() {
        if (this.toggle) {
            this.toggle.addEventListener('change', (e) => {
                const newTheme = e.target.checked ? this.themes.LIGHT : this.themes.DARK;
                this.switchTheme(newTheme);
            });
        }

        // Listen for system theme changes
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            mediaQuery.addEventListener('change', (e) => {
                // Only auto-switch if user hasn't manually set a preference
                if (!localStorage.getItem('booking-theme-user-preference')) {
                    const systemTheme = e.matches ? this.themes.DARK : this.themes.LIGHT;
                    this.switchTheme(systemTheme, false);
                }
            });
        }

        // Keyboard accessibility
        document.addEventListener('keydown', (e) => {
            // Alt + T to toggle theme
            if (e.altKey && e.key.toLowerCase() === 't') {
                e.preventDefault();
                this.toggleTheme();
            }
        });
    }

    switchTheme(newTheme, savePreference = true) {
        if (newTheme === this.currentTheme) return;

        this.currentTheme = newTheme;
        this.updateTheme(newTheme, true);
        this.updateToggleState();
        
        // Save to localStorage
        localStorage.setItem('booking-theme', newTheme);
        
        if (savePreference) {
            localStorage.setItem('booking-theme-user-preference', 'true');
        }

        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: newTheme }
        }));

        // Show toast notification
        this.showThemeChangeNotification(newTheme);
    }

    updateTheme(theme, animate = false) {
        if (animate) {
            // Add transition effect
            this.html.style.transition = 'all 0.3s ease-in-out';
            setTimeout(() => {
                this.html.style.transition = '';
            }, 300);
        }

        this.html.setAttribute('data-bs-theme', theme);
        document.body.className = document.body.className.replace(/theme-\w+/g, '');
        document.body.classList.add(`theme-${theme}`);
    }

    updateToggleState() {
        if (this.toggle) {
            this.toggle.checked = this.currentTheme === this.themes.LIGHT;
            
            // Update ARIA label
            const label = this.currentTheme === this.themes.DARK ? 
                'Switch to light mode' : 'Switch to dark mode';
            this.toggle.setAttribute('aria-label', label);
        }

        // Update theme icons
        this.updateThemeIcons();
    }

    updateThemeIcons() {
        const lightIcon = document.querySelector('.light-icon');
        const darkIcon = document.querySelector('.dark-icon');
        
        if (lightIcon && darkIcon) {
            if (this.currentTheme === this.themes.DARK) {
                lightIcon.style.opacity = '0.5';
                darkIcon.style.opacity = '1';
            } else {
                lightIcon.style.opacity = '1';
                darkIcon.style.opacity = '0.5';
            }
        }
    }

    toggleTheme() {
        const newTheme = this.currentTheme === this.themes.DARK ? 
            this.themes.LIGHT : this.themes.DARK;
        this.switchTheme(newTheme);
    }

    showThemeChangeNotification(theme) {
        // Only show if toastr is available
        if (typeof toastr !== 'undefined') {
            const message = theme === this.themes.DARK ? 
                'Switched to dark mode' : 'Switched to light mode';
            const icon = theme === this.themes.DARK ? '🌙' : '☀️';
            
            toastr.options = {
                "timeOut": "2000",
                "positionClass": "toast-top-right",
                "showDuration": "300",
                "hideDuration": "300"
            };
            
            toastr.info(`${icon} ${message}`);
        }
    }

    // Public method to get current theme
    getCurrentTheme() {
        return this.currentTheme;
    }

    // Public method to check if dark mode
    isDarkMode() {
        return this.currentTheme === this.themes.DARK;
    }
}

// Initialize theme manager when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.themeManager = new ThemeManager();
    
    // Expose theme toggle function globally for backwards compatibility
    window.toggleTheme = () => window.themeManager.toggleTheme();
});

// Handle theme changes for DataTables and other components
window.addEventListener('themeChanged', function(e) {
    // Reinitialize DataTables with new theme if present
    if (typeof DataTable !== 'undefined' && document.querySelector('.dataTable')) {
        setTimeout(() => {
            $('.dataTable').each(function() {
                if ($.fn.DataTable.isDataTable(this)) {
                    $(this).DataTable().destroy();
                    $(this).DataTable({
                        responsive: true,
                        theme: e.detail.theme
                    });
                }
            });
        }, 100);
    }
    
    // Update charts if present
    if (typeof ApexCharts !== 'undefined' && window.chartInstances) {
        window.chartInstances.forEach(chart => {
            chart.updateOptions({
                theme: {
                    mode: e.detail.theme
                }
            });
        });
    }
});