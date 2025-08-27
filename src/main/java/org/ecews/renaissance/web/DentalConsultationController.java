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
import org.ecews.renaissance.domain.DentalConsultation;
import org.ecews.renaissance.dtos.DentalConsultationDto;
import org.ecews.renaissance.services.DentalConsultationService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/dental-consultations")
@RequiredArgsConstructor
public class DentalConsultationController {
    
    private final DentalConsultationService service;
    
    @PostMapping
    public ResponseEntity<DentalConsultation> save(@RequestBody DentalConsultationDto dto) {
        return ResponseEntity.ok(service.save(dto));
    }
    
    @PostMapping("/bulk")
    public ResponseEntity<List<DentalConsultation>> saveAll(@RequestBody List<DentalConsultationDto> dtos) {
        return ResponseEntity.ok(dtos.stream().map(service::save).toList());
    }
    
    @PutMapping("/{id}")
    public ResponseEntity<DentalConsultation> update(@PathVariable UUID id, @RequestBody DentalConsultationDto dto) {
        return ResponseEntity.ok(service.update(id, dto));
    }
    
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> delete(@PathVariable UUID id) {
        service.delete(id);
        return ResponseEntity.ok().build();
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<DentalConsultation> getById(@PathVariable UUID id) {
        return ResponseEntity.ok(service.findById(id));
    }
    
    @GetMapping("/by-patient/{patientId}")
    public ResponseEntity<List<DentalConsultation>> getByPatient(@PathVariable UUID patientId) {
        return ResponseEntity.ok(service.findByPatientId(patientId));
    }
    
    @GetMapping
    public ResponseEntity<List<DentalConsultation>> getAll() {
        return ResponseEntity.ok(service.getAll());
    }
}
