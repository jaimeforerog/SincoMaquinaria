import { describe, it, expect } from 'vitest';
import { equipos } from '../equipos';

describe('Equipos Mock Data', () => {
  describe('Data Structure Validation', () => {
    it('should export an array', () => {
      expect(Array.isArray(equipos)).toBe(true);
    });

    it('should not be empty', () => {
      expect(equipos.length).toBeGreaterThan(0);
    });

    it('should have the expected number of items', () => {
      expect(equipos).toHaveLength(86);
    });
  });

  describe('Individual Item Validation', () => {
    it('all items should have required fields', () => {
      equipos.forEach((equipo, index) => {
        expect(equipo, `Item at index ${index} should have id`).toHaveProperty('id');
        expect(equipo, `Item at index ${index} should have codigo`).toHaveProperty('codigo');
        expect(equipo, `Item at index ${index} should have nombre`).toHaveProperty('nombre');
      });
    });

    it('all ids should be strings', () => {
      equipos.forEach((equipo, index) => {
        expect(
          typeof equipo.id,
          `Item at index ${index} id should be string, got ${typeof equipo.id}`
        ).toBe('string');
      });
    });

    it('all ids should be non-empty', () => {
      equipos.forEach((equipo, index) => {
        expect(
          equipo.id.length,
          `Item at index ${index} should have non-empty id`
        ).toBeGreaterThan(0);
      });
    });

    it('all nombres should be strings', () => {
      equipos.forEach((equipo, index) => {
        expect(
          typeof equipo.nombre,
          `Item at index ${index} nombre should be string`
        ).toBe('string');
      });
    });

    it('all nombres should be non-empty', () => {
      equipos.forEach((equipo, index) => {
        expect(
          equipo.nombre.length,
          `Item at index ${index} should have non-empty nombre`
        ).toBeGreaterThan(0);
      });
    });

    it('all codigos should be strings', () => {
      equipos.forEach((equipo, index) => {
        expect(
          typeof equipo.codigo,
          `Item at index ${index} codigo should be string`
        ).toBe('string');
      });
    });
  });

  describe('Data Uniqueness', () => {
    it('all ids should be unique', () => {
      const ids = equipos.map(e => e.id);
      const uniqueIds = new Set(ids);
      expect(
        uniqueIds.size,
        `Expected ${ids.length} unique ids, but found ${uniqueIds.size}`
      ).toBe(ids.length);
    });

    it('should not have duplicate id values', () => {
      const ids = equipos.map(e => e.id);
      const duplicates = ids.filter((id, index) => ids.indexOf(id) !== index);

      expect(
        duplicates,
        `Found duplicate ids: ${duplicates.join(', ')}`
      ).toHaveLength(0);
    });
  });

  describe('Data Quality', () => {
    it('should have valid Equipo interface structure', () => {
      equipos.forEach((equipo) => {
        // TypeScript will ensure this at compile time,
        // but we verify at runtime for safety
        expect(equipo).toMatchObject({
          id: expect.any(String),
          codigo: expect.any(String),
          nombre: expect.any(String),
        });
      });
    });

    it('should handle optional fields correctly', () => {
      // Verify that optional fields (modelo, serie) are either undefined or strings
      equipos.forEach((equipo, index) => {
        if ('modelo' in equipo && equipo.modelo !== undefined) {
          expect(
            typeof equipo.modelo,
            `Item at index ${index} modelo should be string if present`
          ).toBe('string');
        }

        if ('serie' in equipo && equipo.serie !== undefined) {
          expect(
            typeof equipo.serie,
            `Item at index ${index} serie should be string if present`
          ).toBe('string');
        }
      });
    });
  });

  describe('Specific Data Validation', () => {
    it('should contain known test equipment', () => {
      const conocidos = [
        'ACTIVO SEBAS SIN PADRE',
        'Volqueta Dobletroque international XAB-844',
        'Retroexcavadora JM'
      ];

      conocidos.forEach(nombre => {
        const found = equipos.some(e => e.nombre === nombre);
        expect(
          found,
          `Should contain equipment with nombre: ${nombre}`
        ).toBe(true);
      });
    });

    it('should be able to find equipment by id', () => {
      const equipo = equipos.find(e => e.id === '1');
      expect(equipo).toBeDefined();
      expect(equipo?.nombre).toBe('ACTIVO SEBAS SIN PADRE');
    });

    it('should be able to find equipment by codigo', () => {
      const equipo = equipos.find(e => e.codigo === 'SM002');
      expect(equipo).toBeDefined();
      expect(equipo?.nombre).toBe('Retroexcavadora JM');
    });
  });

  describe('Edge Cases', () => {
    it('should handle equipment with empty codigo gracefully', () => {
      const equipoSinCodigo = equipos.find(e => e.codigo === '');
      // If exists, should still have valid id and nombre
      if (equipoSinCodigo) {
        expect(equipoSinCodigo.id).toBeTruthy();
        expect(equipoSinCodigo.nombre).toBeTruthy();
      }
    });

    it('should not contain null or undefined values in required fields', () => {
      equipos.forEach((equipo, index) => {
        expect(
          equipo.id,
          `Item at index ${index} should not have null/undefined id`
        ).not.toBeNull();
        expect(equipo.id).not.toBeUndefined();

        expect(
          equipo.codigo,
          `Item at index ${index} should not have null/undefined codigo`
        ).not.toBeNull();
        expect(equipo.codigo).not.toBeUndefined();

        expect(
          equipo.nombre,
          `Item at index ${index} should not have null/undefined nombre`
        ).not.toBeNull();
        expect(equipo.nombre).not.toBeUndefined();
      });
    });
  });
});
