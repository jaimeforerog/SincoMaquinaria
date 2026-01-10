/*
  Script para limpiar la base de datos de Marten (SincoMaquinaria)
  -------------------------------------------------------------
  Este script elimina todos los datos de:
  1. Proyecciones (Tablas de lectura)
  2. Event Store (Eventos y Streams)
*/

-- 1. Limpiar tabla de proyección principal (Ordenes de Trabajo)
-- "mt_doc_ordendetrabajo" es el nombre por defecto para la clase OrdenDeTrabajo
TRUNCATE TABLE mt_doc_ordendetrabajo CASCADE;

-- 2. Limpiar el Event Store
-- mt_streams: Tabla maestro de agregados/streams
-- mt_events: Tabla histórica de eventos (se limpia por cascada desde streams)
TRUNCATE TABLE mt_streams CASCADE;

-- En caso de que existan otras tablas de Marten o streams huérfanos:
TRUNCATE TABLE mt_events CASCADE;
TRUNCATE TABLE mt_event_progression CASCADE;

-- Confirmación (Opcional)
SELECT count(*) as total_ordenes FROM mt_doc_ordendetrabajo;
