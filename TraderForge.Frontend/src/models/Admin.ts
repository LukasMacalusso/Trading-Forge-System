import type { SubscriptionPlan } from './Trader';

export type AccountStatus = 'Active' | 'Suspended';

export interface AdminUser {
  id: string;
  email: string;
  status: AccountStatus;
  // The backend trader summary only exposes id/email/suspended/plan, so these
  // are optional and rendered as placeholders when absent.
  userName?: string;
  plan?: SubscriptionPlan;
  registeredAt?: string;
  portfolioValue?: number;
}

export interface AdminPlan {
  id: string;
  name: string;
  monthlyPrice: number;
  initialVirtualBalance: number;
  maxActiveStrategies: number | null;
  maxActiveAssets: number | null;
  canModifyVirtualBalance: boolean;
}

/** Create/update payload — same shape as a plan without its id. */
export type PlanFormData = Omit<AdminPlan, 'id'>;

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
