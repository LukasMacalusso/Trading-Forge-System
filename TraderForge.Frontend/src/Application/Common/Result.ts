/** Mirrors the backend Result<T> pattern for consistent error handling across layers. */
export interface Result<T> {
  isSuccess: boolean;
  value?: T;
  errorMessage?: string;
}

export const Result = {
  ok<T>(value: T): Result<T> {
    return { isSuccess: true, value };
  },
  fail<T>(errorMessage: string): Result<T> {
    return { isSuccess: false, errorMessage };
  },
};
