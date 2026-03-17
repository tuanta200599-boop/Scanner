import { initializeApp } from "https://www.gstatic.com/firebasejs/10.5.0/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/10.5.0/firebase-messaging.js";

const firebaseConfig = {
    apiKey: "AIzaSyC9IgmwPJI80fRjvjO35v3u5Q6zrsm8IM4",
    authDomain: "fir-config-24515.firebaseapp.com",
    projectId: "fir-config-24515",
    storageBucket: "fir-config-24515.appspot.com",
    messagingSenderId: "750710806613",
    appId: "1:750710806613:web:d9c3db08ba1b7b94c89cdc"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

Notification.requestPermission().then(permission => {
    if (permission === 'granted') {
        navigator.serviceWorker.register('/firebase-messaging-sw.js').then(async (registration) => {
            // Đợi cho đến khi Service Worker ở trạng thái 'active'
            const activeWorker = registration.active || registration.waiting || registration.installing;
            
            if (registration.active) {
                console.log('Service Worker is already active');
            } else {
                console.log('Waiting for Service Worker to become active...');
                await new Promise((resolve) => {
                    const worker = registration.installing || registration.waiting;
                    worker.addEventListener('statechange', (e) => {
                        if (e.target.state === 'activated') {
                            resolve();
                        }
                    });
                });
                console.log('Service Worker activated!');
            }

            return getToken(messaging, {
                vapidKey: 'BNBghSaDseuOvHcqZN5rlEVGwKKvsR6252d_Dc1lJ0epdb0B0mCqOP1CYxme_8OeXOh1nQSjQcCSclhZTbjk2i0',
                serviceWorkerRegistration: registration
            });
        }).then(token => {
            console.log('Token:', token);
            // Tự động đăng ký Topic WebAdminBus sử dụng URL từ appsettings.json
            const baseUrl = window.apiBaseUrl || "http://localhost:5437";
            fetch(`${baseUrl}/SubscribeToTopic?Token=${token}&Topics=WebAdminBus`, {
                method: 'POST',
                headers: {
                    'accept': '*/*'
                }
            })
            .then(response => {
                if (response.ok) console.log('✅ Đăng ký Topic WebAdminBus thành công');
                else console.error('❌ Lỗi đăng ký Topic:', response.statusText);
            })
            .catch(err => console.error('❌ Lỗi kết nối API Subscribe:', err));
        }).catch(err => {
            console.error('Error getting token:', err);
        });

        // Lắng nghe tin nhắn từ Service Worker (Background)
        const bc = new BroadcastChannel('fcm_notifications');
        bc.onmessage = (event) => {
            console.log('--- index.js nhận tin nhắn từ BroadcastChannel:', event.data);
            if (event.data && event.data.type === 'FCM_NOTIFICATION') {
                if (typeof window.addNotificationToUI === 'function') {
                    window.addNotificationToUI(event.data.payload);
                } else {
                    console.error('❌ window.addNotificationToUI chưa được định nghĩa!');
                }
            }
        };

        onMessage(messaging, (payload) => {
            console.log('--- index.js nhận Foreground message:', payload);
            if (typeof window.addNotificationToUI === 'function') {
                window.addNotificationToUI(payload);
            } else {
                console.error('❌ window.addNotificationToUI chưa được định nghĩa!');
            }
        });
    }
});