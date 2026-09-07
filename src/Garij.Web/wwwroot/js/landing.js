/**
 * Garij Automotive Landing Page JavaScript
 * Features: Tachometer Preloader, Scroll-Reveal Animations, Smooth Navigation, Quick Lookup
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. Tachometer Preloader Handling
    const preloader = document.getElementById('page-preloader');
    if (preloader) {
        const hidePreloader = () => {
            preloader.classList.add('loaded');
            setTimeout(() => {
                preloader.style.display = 'none';
            }, 650);
        };

        // If page already loaded or on window load
        if (document.readyState === 'complete') {
            setTimeout(hidePreloader, 400);
        } else {
            window.addEventListener('load', () => {
                setTimeout(hidePreloader, 500);
            });
            // Fallback timeout in case assets take long
            setTimeout(hidePreloader, 2500);
        }
    }

    // 2. Scroll Reveal Animations with IntersectionObserver
    const revealElements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right');
    if ('IntersectionObserver' in window) {
        const revealObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('active');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            root: null,
            threshold: 0.12,
            rootMargin: '0px 0px -40px 0px'
        });

        revealElements.forEach(el => revealObserver.observe(el));
    } else {
        // Fallback for older browsers
        revealElements.forEach(el => el.classList.add('active'));
    }

    // 3. Smooth Scrolling for Internal Hash Anchors
    const smoothLinks = document.querySelectorAll('a[href^="#"], a[href^="/#"]');
    smoothLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            let targetId = this.getAttribute('href');
            if (targetId.startsWith('/#')) {
                if (window.location.pathname === '/' || window.location.pathname === '') {
                    targetId = targetId.substring(1);
                } else {
                    return; // Allow natural navigation to /#hash
                }
            }
            if (targetId && targetId !== '#') {
                const targetElement = document.querySelector(targetId);
                if (targetElement) {
                    e.preventDefault();
                    const navOffset = 85;
                    const elementPosition = targetElement.getBoundingClientRect().top;
                    const offsetPosition = elementPosition + window.pageYOffset - navOffset;

                    window.scrollTo({
                        top: offsetPosition,
                        behavior: 'smooth'
                    });
                }
            }
        });
    });

    // 4. Hero Slider Dots Interactive Indicator
    const sliderDots = document.querySelectorAll('.slider-dot');
    sliderDots.forEach((dot, index) => {
        dot.addEventListener('click', () => {
            sliderDots.forEach(d => d.classList.remove('active'));
            dot.classList.add('active');
        });
    });

    // 5. Active Navbar Link on Scroll Spy
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.main-navbar-sticky .nav-link');

    const highlightNavLink = () => {
        let scrollY = window.pageYOffset;
        sections.forEach(section => {
            const sectionHeight = section.offsetHeight;
            const sectionTop = section.offsetTop - 120;
            const sectionId = section.getAttribute('id');

            if (scrollY > sectionTop && scrollY <= sectionTop + sectionHeight) {
                navLinks.forEach(link => {
                    link.classList.remove('active');
                    if (link.getAttribute('href') === `#${sectionId}` || link.getAttribute('href') === `/#${sectionId}`) {
                        link.classList.add('active');
                    }
                });
            }
        });
    };

    window.addEventListener('scroll', highlightNavLink, { passive: true });

    // 6. Auto-scroll on page load if URL contains hash (after preloader dismisses)
    const scrollToTarget = (targetSelector) => {
        try {
            const targetElement = document.querySelector(targetSelector);
            if (targetElement) {
                const navOffset = 85;
                const elementPosition = targetElement.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - navOffset;
                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        } catch (err) {
            // Invalid selector ignore
        }
    };

    if (window.location.hash && window.location.hash !== '#') {
        setTimeout(() => {
            scrollToTarget(window.location.hash);
        }, 600);
    } else if (window.location.search.includes('query=')) {
        setTimeout(() => {
            scrollToTarget('#status-tracker');
        }, 600);
    }
});
