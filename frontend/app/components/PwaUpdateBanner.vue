<template>
  <Transition name="slide-up">
    <div v-if="$pwa?.needRefresh" class="pwa-update-banner">
      <div class="pwa-update-inner">
        <div class="pwa-update-icon">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 12a9 9 0 0 0-9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
            <path d="M3 3v5h5" />
            <path d="M3 12a9 9 0 0 0 9 9 9.75 9.75 0 0 0 6.74-2.74L21 16" />
            <path d="M16 16h5v5" />
          </svg>
        </div>
        <p class="pwa-update-text">A new version is available</p>
        <div class="pwa-update-actions">
          <button @click="$pwa?.closePrompt()" class="pwa-update-dismiss">
            Later
          </button>
          <button @click="$pwa?.updateServiceWorker()" class="pwa-update-reload">
            Update Now
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.pwa-update-banner {
  position: fixed;
  bottom: 5.5rem;
  left: 50%;
  transform: translateX(-50%);
  z-index: 9999;
  width: calc(100% - 2rem);
  max-width: 28rem;
}

@media (min-width: 768px) {
  .pwa-update-banner {
    bottom: 1.5rem;
  }
}

.pwa-update-inner {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-radius: 1rem;
  background: rgba(0, 104, 55, 0.15);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(0, 104, 55, 0.3);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(255, 255, 255, 0.05) inset;
}

.pwa-update-icon {
  flex-shrink: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.5rem;
  background: rgba(0, 104, 55, 0.25);
  color: #4ade80;
}

.pwa-update-text {
  flex: 1;
  font-size: 0.8125rem;
  font-weight: 600;
  color: #e5e7eb;
  letter-spacing: 0.01em;
}

.pwa-update-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.pwa-update-dismiss {
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #9ca3af;
  padding: 0.375rem 0.625rem;
  border-radius: 0.5rem;
  transition: color 0.2s;
}

.pwa-update-dismiss:hover {
  color: #e5e7eb;
}

.pwa-update-reload {
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #fff;
  padding: 0.375rem 0.75rem;
  border-radius: 0.5rem;
  background: linear-gradient(135deg, #006837, #00994d);
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(0, 104, 55, 0.4);
}

.pwa-update-reload:hover {
  background: linear-gradient(135deg, #00994d, #006837);
  box-shadow: 0 4px 12px rgba(0, 104, 55, 0.5);
  transform: translateY(-1px);
}

/* Transition */
.slide-up-enter-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.slide-up-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 1, 1);
}

.slide-up-enter-from {
  opacity: 0;
  transform: translateX(-50%) translateY(1.5rem);
}

.slide-up-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(1.5rem);
}
</style>
