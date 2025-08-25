export type BulkDeleteRequest = { ids: number[] };

export type BulkDeleteResult = {
  deletedIds: number[];
  notFoundIds: number[];
  deletedCount: number;
  notFoundCount: number;
};
