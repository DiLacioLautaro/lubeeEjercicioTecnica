// src/features/publications/services/mappers/toDto.ts
import type { Publication } from '../types/publication';

export type PublicationCreateUpdateDto = {
  tipoPropiedad: string;
  tipoOperacion: string;
  descripcion: string;
  ambientes: number;
  m2: number;
  antiguedad: number;
  lat: number;
  lng: number;
  imagenes: { url: string }[];
};

export function toDto(p: Publication): PublicationCreateUpdateDto {
  const desc = (p.descripcion ?? '').trim();

  const latNum = typeof p.lat === 'number' ? p.lat : Number(p.lat);
  const lngNum = typeof p.lng === 'number' ? p.lng : Number(p.lng);

  return {
    tipoPropiedad: (p.tipoPropiedad ?? '').trim(),
    tipoOperacion: (p.tipoOperacion ?? '').trim(),
    descripcion: desc,
    ambientes: Number.isFinite(+p.ambientes) ? +p.ambientes : 0,
    m2: Number.isFinite(+p.m2) ? +p.m2 : 0,
    antiguedad: Number.isFinite(+p.antiguedad) ? +p.antiguedad : 0,
    lat: latNum,
    lng: lngNum,
    imagenes: (p.imagenes ?? [])
      .map(i => (i?.url ?? '').trim())
      .filter(u => u.length > 0)
      .map(u => ({ url: u })),
  };
}
