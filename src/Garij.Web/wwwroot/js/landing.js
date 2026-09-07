/**
 * Garij Automotive Landing Page JavaScript
 * Features: Tachometer Preloader, Scroll-Reveal Animations, Smooth Navigation, Quick Lookup
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. Lottie Preloader Handling
    const preloader = document.getElementById('page-preloader');
    const lottieContainer = document.getElementById('lottie-preloader');

    if (lottieContainer && window.lottie) {
        try {
            window.lottie.loadAnimation({
                container: lottieContainer,
                renderer: 'svg',
                loop: true,
                autoplay: true,
                path: '/animations/loading.json'
            });
        } catch (e) {
            console.warn('Lottie animation failed to load:', e);
        }
    }

    if (preloader) {
        const startTime = Date.now();
        const minDisplayMs = 2100; // Ensure cursive 'garij' animation completes its trace

        const hidePreloader = () => {
            const elapsed = Date.now() - startTime;
            const remaining = Math.max(0, minDisplayMs - elapsed);
            setTimeout(() => {
                preloader.classList.add('loaded');
                setTimeout(() => {
                    preloader.style.display = 'none';
                }, 650);
            }, remaining);
        };

        if (document.readyState === 'complete') {
            hidePreloader();
        } else {
            window.addEventListener('load', hidePreloader);
            // Fallback safety timeout
            setTimeout(hidePreloader, 3200);
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
    const smoothLinks = document.querySelectorAll('a[href^="#"]');
    smoothLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId && targetId !== '#') {
                const targetElement = document.querySelector(targetId);
                if (targetElement) {
                    e.preventDefault();
                    
                    // If inside offcanvas, close it smoothly before scrolling
                    const isInsideOffcanvas = this.closest('.offcanvas');
                    const delay = isInsideOffcanvas ? 280 : 0;

                    setTimeout(() => {
                        const navOffset = 80;
                        const elementPosition = targetElement.getBoundingClientRect().top;
                        const offsetPosition = elementPosition + window.pageYOffset - navOffset;

                        window.scrollTo({
                            top: offsetPosition,
                            behavior: 'smooth'
                        });
                    }, delay);
                }
            }
        });
    });

    // 4. Hero Multi-Slide Navigation Controls (Matching Mockup Arrows)
    const heroSlides = document.querySelectorAll('.hero-slide-item');
    const prevBtn = document.getElementById('heroPrevBtn');
    const nextBtn = document.getElementById('heroNextBtn');
    const indicators = document.querySelectorAll('.hero-slide-indicators .slide-indicator');
    let currentSlide = 0;
    let slideTimer = null;

    if (heroSlides.length > 0) {
        const showSlide = (index) => {
            if (index < 0) index = heroSlides.length - 1;
            if (index >= heroSlides.length) index = 0;
            currentSlide = index;

            heroSlides.forEach((slide, i) => {
                if (i === currentSlide) {
                    slide.classList.add('active');
                } else {
                    slide.classList.remove('active');
                }
            });

            indicators.forEach((dot, i) => {
                if (i === currentSlide) {
                    dot.classList.add('active');
                } else {
                    dot.classList.remove('active');
                }
            });
        };

        const nextSlide = () => showSlide(currentSlide + 1);
        const prevSlide = () => showSlide(currentSlide - 1);

        const resetSlideTimer = () => {
            if (slideTimer) clearInterval(slideTimer);
            slideTimer = setInterval(nextSlide, 8500); // Rotates every 8.5 seconds
        };

        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                nextSlide();
                resetSlideTimer();
            });
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                prevSlide();
                resetSlideTimer();
            });
        }

        indicators.forEach((dot, idx) => {
            dot.addEventListener('click', () => {
                showSlide(idx);
                resetSlideTimer();
            });
        });

        // Pause rotation on hover so user can read/click smoothly
        const heroSection = document.getElementById('hero');
        if (heroSection) {
            heroSection.addEventListener('mouseenter', () => {
                if (slideTimer) clearInterval(slideTimer);
            });
            heroSection.addEventListener('mouseleave', () => {
                resetSlideTimer();
            });
        }

        // Resume rotation when window/tab is active
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                if (slideTimer) clearInterval(slideTimer);
            } else {
                resetSlideTimer();
            }
        });

        // Initialize auto rotation
        resetSlideTimer();

        // Keyboard arrow navigation
        window.addEventListener('keydown', (e) => {
            if (window.scrollY < window.innerHeight * 0.8) {
                if (e.key === 'ArrowLeft') {
                    prevSlide();
                    resetSlideTimer();
                } else if (e.key === 'ArrowRight') {
                    nextSlide();
                    resetSlideTimer();
                }
            }
        });
    }

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
                    if (link.getAttribute('href') === `#${sectionId}`) {
                        link.classList.add('active');
                    }
                });
            }
        });
    };

    window.addEventListener('scroll', highlightNavLink, { passive: true });

    // 6. Auto-scroll to #status-tracker if navigated or query exists
    if (window.location.hash === '#status-tracker' || window.location.search.includes('query=')) {
        setTimeout(() => {
            const trackerSection = document.getElementById('status-tracker');
            if (trackerSection) {
                const navOffset = 80;
                const elementPosition = trackerSection.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - navOffset;
                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        }, 500);
    }
});
