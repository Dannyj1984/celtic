/// <reference lib="webworker" />

self.addEventListener('push', (event) => {
  if (!(self.Notification && self.Notification.permission === 'granted')) {
    return;
  }

  const data = event.data?.json() ?? {
    notification: {
      title: 'Celtic FC Update',
      body: 'You have a new update from the team.',
      icon: '/pwa-192x192.png'
    }
  };

  const title = data.notification.title;
  const options = {
    body: data.notification.body,
    icon: data.notification.icon || '/pwa-192x192.png',
    badge: '/favicon.ico',
    data: data.notification.data || { url: '/' }
  };

  event.waitUntil(
    self.registration.showNotification(title, options)
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const urlToOpen = event.notification.data.url || '/';

  event.waitUntil(
    self.clients.matchAll({
      type: 'window',
      includeUncontrolled: true
    }).then((windowClients) => {
      for (let i = 0; i < windowClients.length; i++) {
        const client = windowClients[i];
        if (client.url === urlToOpen && 'focus' in client) {
          return client.focus();
        }
      }
      if (self.clients.openWindow) {
        return self.clients.openWindow(urlToOpen);
      }
    })
  );
});
