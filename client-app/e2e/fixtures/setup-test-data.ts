import { Page } from '@playwright/test';
import { testData, generateUniqueEquipo, generateUniqueRutina } from './test-data';
import { getAuthToken } from '../utils/helpers';

/**
 * Test Data Setup Script
 *
 * Utilities for populating the database with test data before running E2E tests.
 * This script can be used in test fixtures or setup hooks.
 */

/**
 * Setup basic test data (admin user, sample equipos, rutinas)
 */
export async function setupBasicTestData(page: Page) {
  const token = await getAuthToken(page);

  if (!token) {
    throw new Error('Must be authenticated to setup test data');
  }

  const createdIds: {
    equipos: string[];
    rutinas: string[];
    orders: string[];
  } = {
    equipos: [],
    rutinas: [],
    orders: [],
  };

  try {
    // Create test rutinas first (needed for preventive orders)
    for (const rutina of testData.rutinas) {
      const uniqueRutina = generateUniqueRutina(rutina);
      const rutinaId = await createRutina(page, uniqueRutina, token);
      createdIds.rutinas.push(rutinaId);
    }

    // Create test equipos
    for (const equipo of testData.equipos) {
      const uniqueEquipo = generateUniqueEquipo(equipo);
      const equipoId = await createEquipo(page, uniqueEquipo, token);
      createdIds.equipos.push(equipoId);
    }

    return createdIds;
  } catch (error) {
    console.error('Error setting up test data:', error);
    // Cleanup on error
    await cleanupTestData(page, createdIds, token);
    throw error;
  }
}

/**
 * Create a single equipo
 */
async function createEquipo(page: Page, equipoData: any, token: string): Promise<string> {
  const response = await page.request.post('/api/equipos', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: {
      Placa: equipoData.placa,
      Descripcion: equipoData.descripcion,
      Marca: equipoData.marca,
      Modelo: equipoData.modelo,
      Serie: equipoData.serie,
      Año: equipoData.año,
      HorasUso: equipoData.horasUso,
    },
  });

  if (!response.ok()) {
    const errorText = await response.text();
    throw new Error(`Failed to create equipo: ${response.status()} - ${errorText}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Create a single rutina with activities
 */
async function createRutina(page: Page, rutinaData: any, token: string): Promise<string> {
  const response = await page.request.post('/api/rutinas', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: {
      Nombre: rutinaData.nombre,
      Descripcion: rutinaData.descripcion,
      Partes: rutinaData.actividades ? [
        {
          Nombre: 'Parte Principal',
          Actividades: rutinaData.actividades.map((act: any) => ({
            Nombre: act.nombre,
            Descripcion: act.descripcion,
            Frecuencia: 30, // Monthly by default
          })),
        },
      ] : [],
    },
  });

  if (!response.ok()) {
    const errorText = await response.text();
    throw new Error(`Failed to create rutina: ${response.status()} - ${errorText}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Create a test order
 */
export async function createTestOrder(
  page: Page,
  equipoId: string,
  tipo: 'Preventivo' | 'Correctivo',
  rutinaId?: string,
  token?: string
): Promise<string> {
  if (!token) {
    token = await getAuthToken(page) || '';
  }

  const orderData: any = {
    Numero: `OT-E2E-${Date.now()}`,
    EquipoId: equipoId,
    Origen: 'Manual',
    Tipo: tipo,
    FechaOrden: new Date().toISOString(),
  };

  if (tipo === 'Preventivo' && rutinaId) {
    orderData.RutinaId = rutinaId;
    orderData.FrecuenciaPreventiva = 30; // Monthly
  }

  const response = await page.request.post('/api/ordenes', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: orderData,
  });

  if (!response.ok()) {
    const errorText = await response.text();
    throw new Error(`Failed to create order: ${response.status()} - ${errorText}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Cleanup test data by IDs
 */
async function cleanupTestData(
  page: Page,
  ids: { equipos: string[]; rutinas: string[]; orders: string[] },
  token: string
) {
  // Delete in reverse order to avoid foreign key issues
  for (const orderId of ids.orders) {
    try {
      await page.request.delete(`/api/ordenes/${orderId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete order ${orderId}:`, error);
    }
  }

  for (const equipoId of ids.equipos) {
    try {
      await page.request.delete(`/api/equipos/${equipoId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete equipo ${equipoId}:`, error);
    }
  }

  for (const rutinaId of ids.rutinas) {
    try {
      await page.request.delete(`/api/rutinas/${rutinaId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete rutina ${rutinaId}:`, error);
    }
  }
}

/**
 * Cleanup all test data (equipos with E2E- or TEST- prefix)
 */
export async function cleanupAllTestData(page: Page) {
  const token = await getAuthToken(page);

  if (!token) {
    console.warn('No auth token found, skipping cleanup');
    return;
  }

  try {
    // Get and delete test equipos
    const equiposResponse = await page.request.get('/api/equipos', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (equiposResponse.ok()) {
      const equipos = await equiposResponse.json();
      const data = equipos.data || equipos;

      for (const equipo of data) {
        if (equipo.placa.startsWith('E2E-') || equipo.placa.startsWith('TEST-')) {
          try {
            await page.request.delete(`/api/equipos/${equipo.id}`, {
              headers: { 'Authorization': `Bearer ${token}` },
            });
          } catch (error) {
            console.warn(`Failed to delete equipo ${equipo.id}:`, error);
          }
        }
      }
    }

    // Get and delete test rutinas
    const rutinasResponse = await page.request.get('/api/rutinas', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (rutinasResponse.ok()) {
      const rutinas = await rutinasResponse.json();
      const data = rutinas.data || rutinas;

      for (const rutina of data) {
        if (rutina.nombre.includes('E2E') || rutina.nombre.includes('Test')) {
          try {
            await page.request.delete(`/api/rutinas/${rutina.id}`, {
              headers: { 'Authorization': `Bearer ${token}` },
            });
          } catch (error) {
            console.warn(`Failed to delete rutina ${rutina.id}:`, error);
          }
        }
      }
    }
  } catch (error) {
    console.error('Error during cleanup:', error);
  }
}

/**
 * Create complete test scenario (equipos, rutinas, orders)
 */
export async function createCompleteTestScenario(page: Page) {
  const ids = await setupBasicTestData(page);

  // Create some test orders
  const token = await getAuthToken(page) || '';

  if (ids.equipos.length > 0 && ids.rutinas.length > 0) {
    // Create a preventive order
    const preventiveOrderId = await createTestOrder(
      page,
      ids.equipos[0],
      'Preventivo',
      ids.rutinas[0],
      token
    );
    ids.orders.push(preventiveOrderId);

    // Create a corrective order if we have another equipo
    if (ids.equipos.length > 1) {
      const correctiveOrderId = await createTestOrder(
        page,
        ids.equipos[1],
        'Correctivo',
        undefined,
        token
      );
      ids.orders.push(correctiveOrderId);
    }
  }

  return ids;
}
