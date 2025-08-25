import { computed } from 'vue'

const basePropertyTypes = ['Casa', 'Departamento', 'PH', 'Terreno'] as const
const baseOperationTypes = ['Venta', 'Alquiler'] as const

export function usePublicationOptions() {
  const propertyTypes = computed(() =>
    basePropertyTypes.map(v => ({ label: v, value: v }))
  )
  const operationTypes = computed(() =>
    baseOperationTypes.map(v => ({ label: v, value: v }))
  )
  return { propertyTypes, operationTypes }
}
