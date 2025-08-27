/*
* MIT License
*
* Copyright (c) 2025 [AUTHOR OR ORGANIZATION]
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
package org.ecews.renaissance.web;

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.PharmacyPrescription;
import org.ecews.renaissance.dtos.PharmacyPrescriptionDto;
import org.ecews.renaissance.services.PharmacyPrescriptionService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/pharmacy")
@RequiredArgsConstructor
public class PharmacyPrescriptionController {
    
    private final PharmacyPrescriptionService service;
    
    @PostMapping("/bulk")
    public ResponseEntity<Void> savePrescriptions(@RequestBody List<PharmacyPrescriptionDto> prescriptions) {
        service.saveAll(prescriptions);
        return ResponseEntity.ok().build();
    }
    
    @PutMapping("/bulk")
    public ResponseEntity<Void> updatePrescriptions(@RequestBody List<PharmacyPrescriptionDto> prescriptions) {
        service.updateAll(prescriptions);
        return ResponseEntity.ok().build();
    }
    
    @GetMapping
    public List<PharmacyPrescriptionDto> getAll() {
        return service.getAllPrescriptions();
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<PharmacyPrescriptionDto> get(@PathVariable UUID id) {
        return ResponseEntity.ok(service.getById(id));
    }
    
    @GetMapping("/patient/{patientId}")
    public List<PharmacyPrescriptionDto> getByPatient(@PathVariable UUID patientId) {
        return service.getByPatientId(patientId);
    }
    
    @PostMapping
    public ResponseEntity<PharmacyPrescriptionDto> create(@RequestBody PharmacyPrescriptionDto dto) {
        return ResponseEntity.ok(service.create(dto));
    }
    
    @PutMapping("/{id}")
    public ResponseEntity<PharmacyPrescriptionDto> update(@PathVariable UUID id, @RequestBody PharmacyPrescriptionDto dto) {
        return ResponseEntity.ok(service.update(id, dto));
    }
    
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> archive(@PathVariable UUID id) {
        service.archive(id);
        return ResponseEntity.noContent().build();
    }
    
    @GetMapping("/has-pending/{patientId}")
    public ResponseEntity<Boolean> hasPending(@PathVariable UUID patientId) {
        boolean hasPending = service.hasPendingPrescriptions(patientId);
        return ResponseEntity.ok(hasPending);
    }
    
}
