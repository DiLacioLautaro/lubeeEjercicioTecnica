export interface PublicationImage {
  id?: number
  url: string
  publicationId?: number
}

export interface Publication {
  id?: number
  tipoPropiedad: string
  tipoOperacion: string
  descripcion: string
  ambientes: number
  m2: number
  antiguedad: number
  lat: number | null    
  lng: number | null   
  imagenes: PublicationImage[]
}
