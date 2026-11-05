import type { ExpoConfig } from 'expo/config';

const apiUrl = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:5080';
const apiMock = (process.env.EXPO_PUBLIC_API_MOCK ?? '1') !== '0';

const config: ExpoConfig = {
  name: 'Termômetro da Sala',
  slug: 'termometro-da-sala',
  version: '0.1.0',
  orientation: 'portrait',
  icon: './assets/icon.png',
  scheme: 'termometro',
  userInterfaceStyle: 'light',
  ios: { supportsTablet: true },
  android: {
    adaptiveIcon: {
      backgroundColor: '#E6F4FE',
      foregroundImage: './assets/android-icon-foreground.png',
      backgroundImage: './assets/android-icon-background.png',
      monochromeImage: './assets/android-icon-monochrome.png',
    },
  },
  web: { favicon: './assets/favicon.png' },
  plugins: [
    'expo-router',
    'expo-status-bar',
    [
      'expo-image-picker',
      {
        cameraPermission: 'A câmera fotografa a sala ou o equipamento, sem pessoas.',
        microphonePermission: false,
      },
    ],
  ],
  extra: { apiUrl, apiMock },
};

export default config;
