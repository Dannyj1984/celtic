export default defineNuxtRouteMiddleware((to) => {
  const { isAuthenticated, init } = useAuth()

  init()

  // Public routes that don't need auth
  const publicRoutes = ['/login']
  if (publicRoutes.includes(to.path)) {
    // Redirect to dashboard if already logged in
    if (isAuthenticated.value) {
      return navigateTo('/dashboard')
    }
    return
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated.value) {
    return navigateTo('/login')
  }
})
