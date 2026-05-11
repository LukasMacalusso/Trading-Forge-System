import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';

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

export class SubscriptionService {
  async getMyPlan(): Promise<Result<PlanInfo>> {
    try {
      const { data } = await httpClient.get<TraderPlanResponse>('/api/subscription/trader-plan');
      return Result.ok(data.plan);
    } catch (error) {
      const e = error as { response?: { data?: { error?: string } }; code?: string };
      if (e?.code === 'ERR_NETWORK' || !e?.response) return Result.fail('Cannot reach the server.');
      if (e?.response?.data?.error) return Result.fail(e.response.data.error);
      return Result.fail('Failed to load plan.');
    }
  }
}
