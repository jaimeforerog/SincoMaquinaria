/**
 * Test data fixtures for E2E testing
 *
 * This file contains all test data used across E2E tests including:
 * - Test users with credentials
 * - Test equipos with all required fields
 * - Test rutinas with activities
 * - Test empleados
 *
 * Use unique prefixes (E2E-, TEST-) to avoid conflicts with real data
 */

import { TEST_USER, TEST_PREFIXES } from '../e2e.config';

export const testData = {
  // Test users
  users: {
    admin: {
      email: TEST_USER.email,
      password: TEST_USER.password,
      nombre: TEST_USER.nombre,
    },
    // Add more test users as needed
    mechanic: {
      email: 'mechanic@test.com',
      password: 'Mechanic123!',
      nombre: 'Mechanic Test User',
    },
  },

  // Test equipos (equipment)
  equipos: [
    {
      placa: `${TEST_PREFIXES.equipo}001`,
      descripcion: 'Excavadora Test E2E',
      tipo: 'Excavadora',
      marca: 'CAT',
      modelo: '320D',
      serie: `SER-${TEST_PREFIXES.equipo}001`,
      año: 2023,
      horasUso: 1000,
    },
    {
      placa: `${TEST_PREFIXES.equipo}002`,
      descripcion: 'Bulldozer Test E2E',
      tipo: 'Bulldozer',
      marca: 'Komatsu',
      modelo: 'D65PX',
      serie: `SER-${TEST_PREFIXES.equipo}002`,
      año: 2022,
      horasUso: 1500,
    },
    {
      placa: `${TEST_PREFIXES.equipo}003`,
      descripcion: 'Grúa Test E2E',
      tipo: 'Grúa',
      marca: 'Liebherr',
      modelo: 'LTM 1100',
      serie: `SER-${TEST_PREFIXES.equipo}003`,
      año: 2024,
      horasUso: 500,
    },
  ],

  // Test rutinas (maintenance routines)
  rutinas: [
    {
      nombre: `${TEST_PREFIXES.rutina}Preventivo`,
      descripcion: 'Rutina de prueba para E2E testing',
      grupo: 'Mantenimiento Preventivo',
      actividades: [
        {
          nombre: 'Cambio de aceite motor',
          descripcion: 'Cambio completo de aceite del motor',
        },
        {
          nombre: 'Revisión de frenos',
          descripcion: 'Inspección y ajuste de sistema de frenos',
        },
        {
          nombre: 'Inspección de filtros',
          descripcion: 'Revisión y reemplazo de filtros de aire y combustible',
        },
      ],
    },
    {
      nombre: `${TEST_PREFIXES.rutina}Diario`,
      descripcion: 'Inspección diaria de rutina',
      grupo: 'Inspección Diaria',
      actividades: [
        {
          nombre: 'Revisión de niveles',
          descripcion: 'Verificar niveles de fluidos',
        },
        {
          nombre: 'Inspección visual',
          descripcion: 'Inspección visual general del equipo',
        },
      ],
    },
  ],

  // Test empleados (employees)
  empleados: [
    {
      nombre: `${TEST_PREFIXES.empleado}Juan Pérez`,
      cedula: `${TEST_PREFIXES.empleado}123456`,
      cargo: 'Mecánico',
      email: 'juan.perez.e2e@test.com',
      telefono: '3001234567',
    },
    {
      nombre: `${TEST_PREFIXES.empleado}María González`,
      cedula: `${TEST_PREFIXES.empleado}789012`,
      cargo: 'Supervisor',
      email: 'maria.gonzalez.e2e@test.com',
      telefono: '3007890123',
    },
  ],

  // Order types
  orderTypes: {
    preventivo: 'Preventivo',
    correctivo: 'Correctivo',
  },

  // Frequencies for preventive maintenance
  frequencies: {
    daily: 'Diaria',
    weekly: 'Semanal',
    monthly: 'Mensual',
    quarterly: 'Trimestral',
    annual: 'Anual',
  },

  // Failure types for corrective maintenance
  failureTypes: {
    mechanical: 'Falla Mecánica',
    electrical: 'Falla Eléctrica',
    hydraulic: 'Falla Hidráulica',
  },

  // Activity progress values
  progressValues: [0, 25, 50, 75, 100],
};

/**
 * Generate unique test data with timestamp to avoid conflicts
 */
export function generateUniqueEquipo(base: typeof testData.equipos[0]) {
  const timestamp = Date.now();
  return {
    ...base,
    placa: `${base.placa}-${timestamp}`,
    serie: `${base.serie}-${timestamp}`,
  };
}

export function generateUniqueRutina(base: typeof testData.rutinas[0]) {
  const timestamp = Date.now();
  return {
    ...base,
    nombre: `${base.nombre} ${timestamp}`,
  };
}

export function generateUniqueEmpleado(base: typeof testData.empleados[0]) {
  const timestamp = Date.now();
  return {
    ...base,
    nombre: `${base.nombre} ${timestamp}`,
    cedula: `${base.cedula}-${timestamp}`,
    email: `e2e.${timestamp}@test.com`,
  };
}
