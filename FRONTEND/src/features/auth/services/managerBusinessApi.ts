import api from "../../../core/api/api";

export type ManagerBusinessRoute = {
  businessType: string;
  targetPath: string;
  hasEligibleRental: boolean;
};

export async function getManagerBusinessRoute(): Promise<ManagerBusinessRoute> {
  return api.get("/manager-business/route");
}
