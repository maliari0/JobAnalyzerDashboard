export interface OAuthToken {
  id: number;
  profileId: number;
  provider: string;
  email: string;
  expiresAt: string;
}
