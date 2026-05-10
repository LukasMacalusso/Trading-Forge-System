export interface Trader {
  id: string;
  email: string;
  userName: string;
  freeTrialRegistrationDate: string;
  freeTrialExpirationDate: string;
  plan: SubscriptionPlan;
  virtualBalance: number;
}

export type SubscriptionPlan = 'FreeTrial' | 'Basic' | 'Pro' | 'Enterprise';
