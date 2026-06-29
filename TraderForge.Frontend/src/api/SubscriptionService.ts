import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

export interface PlanInfo {
  id: string;
  name: string;
  monthlyPrice: number;
  initialVirtualBalance: number;
  maxActiveStrategies: number | null;
  maxActiveAssets: number | null;
  canModifyVirtualBalance: boolean;
}

interface TraderPlanResponse {
  plan: PlanInfo;
}

export interface ChangePlanResult {
  message: string;
  discount?: unknown;
}

export class SubscriptionService {
  async getMyPlan(): Promise<Result<PlanInfo>> {
    try {
      const { data } = await httpClient.get<TraderPlanResponse>('/api/subscription/trader-plan');
      return Result.ok(data.plan);
    } catch (error) {
      return Result.fail(toError(error, 'Failed to load plan.'));
    }
  }

  async getPlans(): Promise<Result<PlanInfo[]>> {
    try {
      const { data } = await httpClient.get<PlanInfo[]>('/api/subscription/plans');
      return Result.ok(data);
    } catch (error) {
      return Result.fail(toError(error, 'Failed to load plans.'));
    }
  }

  async changePlan(newPlanId: string, promoCode?: string): Promise<Result<ChangePlanResult>> {
    try {
      const { data } = await httpClient.post<ChangePlanResult>('/api/subscription/pay', {
        newPlanId,
        promoCode: promoCode || null,
      });
      return Result.ok(data);
    } catch (error) {
      return Result.fail(toError(error, 'Failed to change plan.'));
    }
  }
}

function toError(error: unknown, fallback: string): string {
  const e = error as { response?: { data?: { error?: string } }; code?: string };
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'Cannot reach the server.';
  if (e?.response?.data?.error) return e.response.data.error;
  return fallback;
}
