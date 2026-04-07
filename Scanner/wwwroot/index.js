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

// Đăng ký Service Worker ngay lập tức để thỏa mãn điều kiện PWA Installable
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/firebase-messaging-sw.js').then(async (registration) => {
        console.log('✅ Service Worker registered for PWA');

        // Logic xử lý FCM và Token sau khi SW đã sẵn sàng
        Notification.requestPermission().then(async (permission) => {
            if (permission === 'granted') {
                try {
                    const token = await getToken(messaging, {
                        vapidKey: 'BNBghSaDseuOvHcqZN5rlEVGwKKvsR6252d_Dc1lJ0epdb0B0mCqOP1CYxme_8OeXOh1nQSjQcCSclhZTbjk2i0',
                        serviceWorkerRegistration: registration
                    });

                    console.log('Token:', token);
                    fetch(`/Home/SubscribeToTopic?token=${token}&topics=WebAdminBus`, {
                        method: 'POST',
                        headers: {
                            'accept': 'application/json'
                        }
                    })
                        .then(response => response.json())
                        .then(result => {
                            if (result.success) console.log('✅ Đăng ký Topic WebAdminBus thành công');
                            else console.error('❌ Lỗi đăng ký Topic:', result.message);
                        })
                        .catch(err => console.error('❌ Lỗi kết nối API Subscribe:', err));

                } catch (err) {
                    console.error('Error getting token:', err);
                }
            }
        });

        // Lắng nghe tin nhắn từ Service Worker (Background)
        const bc = new BroadcastChannel('fcm_notifications');
        bc.onmessage = (event) => {
            console.log('--- index.js nhận tin nhắn từ BroadcastChannel:', event.data);
            if (event.data && event.data.type === 'FCM_NOTIFICATION') {
                if (typeof window.addNotificationToUI === 'function') {
                    window.addNotificationToUI(event.data.payload);
                }
            }
        };

        // Lắng nghe tin nhắn khi App đang mở (Foreground)
        onMessage(messaging, (payload) => {
            console.log('--- index.js nhận Foreground message:', payload);
            if (typeof window.addNotificationToUI === 'function') {
                window.addNotificationToUI(payload);
            }
        });

    }).catch(err => {
        console.error('❌ Service Worker registration failed:', err);
    });
}
