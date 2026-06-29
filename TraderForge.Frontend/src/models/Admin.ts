import type { SubscriptionPlan } from './Trader';

export type AccountStatus = 'Active' | 'Suspended';

export interface AdminUser {
  id: string;
  email: string;
  userName: string;
  plan: SubscriptionPlan;
  status: AccountStatus;
  registeredAt: string;
  portfolioValue: number;
}

export type RefundStatus = 'Pending' | 'Approved' | 'Rejected';

export interface RefundRequest {
  id: string;
  userEmail: string;
  plan: SubscriptionPlan;
  amount: number;
  reason: string;
  requestedAt: string;
  status: RefundStatus;
}
