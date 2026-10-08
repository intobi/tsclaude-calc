export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  expiresAtUtc: string;
}

export interface AuthSession {
  token: string;
  email: string;
  expiresAtUtc: string;
}
