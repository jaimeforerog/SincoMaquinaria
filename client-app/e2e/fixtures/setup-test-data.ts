import { Page } from '@playwright/test';
import { testData, generateUniqueEquipo, generateUniqueRutina } from './test-data';
import { getAuthToken, retryWithBackoff } from '../utils/helpers';

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
  // Wait for token to be available
  let token = await getAuthToken(page);
  let retries = 0;
  const maxRetries = 20; // 10 seconds total (20 * 500ms)

  while (!token && retries < maxRetries) {
    await page.waitForTimeout(500);
    token = await getAuthToken(page);
    retries++;
  }

  if (!token) {
    throw new Error('Must be authenticated to setup test data. Token not found after 10s');
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
      const rutinaId = await retryWithBackoff(
        () => createRutina(page, uniqueRutina, token),
        2, // 2 retries
        500 // 500ms initial delay
      );
      createdIds.rutinas.push(rutinaId);
    }

    // Create test equipos
    for (const equipo of testData.equipos) {
      const uniqueEquipo = generateUniqueEquipo(equipo);
      const equipoId = await retryWithBackoff(
        () => createEquipo(page, uniqueEquipo, token),
        2, // 2 retries
        500 // 500ms initial delay
      );
      createdIds.equipos.push(equipoId);
    }

    return createdIds;
  } catch (error) {
    console.error('[Setup] Error setting up test data:', error);
    // Cleanup on error
    await cleanupTestData(page, createdIds, token);
    throw error;
  }
}

/**
 * Create a single equipo
 */
async function createEquipo(page: Page, equipoData: any, token: string): Promise<string> {
  const response = await page.request.post('/equipos', {
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
    console.error(`[Setup] Failed to create equipo ${equipoData.placa}: ${response.status()} - ${errorText}`);
    throw new Error(`Failed to create equipo ${equipoData.placa}: ${response.status()} - ${errorText}`);
  }

  const data = await response.json();
  console.log(`[Setup] Created equipo: ${equipoData.placa} (${data.id || data.Id})`);
  return data.id || data.Id;
}

/**
 * Create a single rutina with activities
 */
async function createRutina(page: Page, rutinaData: any, token: string): Promise<string> {
  const response = await page.request.post('/rutinas', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: {
      Nombre: rutinaData.nombre,
      Descripcion: rutinaData.descripcion,
      Grupo: rutinaData.grupo || 'General',
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
    console.error(`[Setup] Failed to create rutina ${rutinaData.nombre}: ${response.status()} - ${errorText}`);
    throw new Error(`Failed to create rutina ${rutinaData.nombre}: ${response.status()} - ${errorText}`);
  }

  const data = await response.json();
  console.log(`[Setup] Created rutina: ${rutinaData.nombre} (${data.id || data.Id})`);
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

  const response = await page.request.post('/ordenes', {
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
      await page.request.delete(`/ordenes/${orderId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete order ${orderId}:`, error);
    }
  }

  for (const equipoId of ids.equipos) {
    try {
      await page.request.delete(`/equipos/${equipoId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete equipo ${equipoId}:`, error);
    }
  }

  for (const rutinaId of ids.rutinas) {
    try {
      await page.request.delete(`/rutinas/${rutinaId}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch (error) {
      console.warn(`Failed to delete rutina ${rutinaId}:`, error);
    }
  }
}

/**
 * Cleanup all test data (equipos with E2E- or TEST- prefix)
 * Deletes in correct order to avoid foreign key constraints: orders -> equipos -> rutinas
 */
export async function cleanupAllTestData(page: Page) {
  const token = await getAuthToken(page);

  if (!token) {
    console.warn('No auth token found, skipping cleanup');
    return;
  }

  try {
    console.log('[Cleanup] Starting test data cleanup...');

    // Step 1: Delete test orders FIRST (to avoid FK constraints)
    try {
      const ordenesResponse = await page.request.get('/ordenes', {
        headers: { 'Authorization': `Bearer ${token}` },
      });

      if (ordenesResponse.ok()) {
        const ordenes = await ordenesResponse.json();
        const data = ordenes.data || ordenes;

        let deletedOrders = 0;
        for (const orden of data) {
          if (orden.numero && (orden.numero.includes('E2E') || orden.numero.includes('OT-E2E'))) {
            try {
              const deleteResponse = await page.request.delete(`/ordenes/${orden.id}`, {
                headers: { 'Authorization': `Bearer ${token}` },
              });
              if (deleteResponse.ok()) deletedOrders++;
            } catch (error) {
              console.warn(`[Cleanup] Failed to delete order ${orden.id}:`, error);
            }
          }
        }
        if (deletedOrders > 0) console.log(`[Cleanup] Deleted ${deletedOrders} test orders`);
      }
    } catch (error) {
      console.warn('[Cleanup] Error cleaning orders (may not exist):', error);
    }

    // Step 2: Delete test equipos
    const equiposResponse = await page.request.get('/equipos', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (equiposResponse.ok()) {
      const equipos = await equiposResponse.json();
      const data = equipos.data || equipos;

      let deletedEquipos = 0;
      for (const equipo of data) {
        if (equipo.placa && (equipo.placa.startsWith('E2E-') || equipo.placa.startsWith('TEST-'))) {
          try {
            const deleteResponse = await page.request.delete(`/equipos/${equipo.id}`, {
              headers: { 'Authorization': `Bearer ${token}` },
            });
            if (deleteResponse.ok()) deletedEquipos++;
          } catch (error) {
            console.warn(`[Cleanup] Failed to delete equipo ${equipo.id} (${equipo.placa}):`, error);
          }
        }
      }
      if (deletedEquipos > 0) console.log(`[Cleanup] Deleted ${deletedEquipos} test equipos`);
    }

    // Step 3: Delete test rutinas LAST
    const rutinasResponse = await page.request.get('/rutinas', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (rutinasResponse.ok()) {
      const rutinas = await rutinasResponse.json();
      const data = rutinas.data || rutinas;

      let deletedRutinas = 0;
      for (const rutina of data) {
        if (rutina.nombre && (rutina.nombre.includes('E2E') || rutina.nombre.includes('Test'))) {
          try {
            const deleteResponse = await page.request.delete(`/rutinas/${rutina.id}`, {
              headers: { 'Authorization': `Bearer ${token}` },
            });
            if (deleteResponse.ok()) deletedRutinas++;
          } catch (error) {
            console.warn(`[Cleanup] Failed to delete rutina ${rutina.id} (${rutina.nombre}):`, error);
          }
        }
      }
      if (deletedRutinas > 0) console.log(`[Cleanup] Deleted ${deletedRutinas} test rutinas`);
    }

    console.log('[Cleanup] Test data cleanup completed');
  } catch (error) {
    console.error('[Cleanup] Error during cleanup:', error);
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
