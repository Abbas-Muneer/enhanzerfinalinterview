export interface AuditLog {
  id: number;
  entity: string;
  action: string;
  oldValue?: string | null;
  newValue?: string | null;
  createdAt: string;
}
