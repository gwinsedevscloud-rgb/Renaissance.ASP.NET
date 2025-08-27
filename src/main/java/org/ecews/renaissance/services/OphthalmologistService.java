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
package org.ecews.renaissance.services;

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.Ophthalmologist;
import org.ecews.renaissance.dtos.OphthalmologistDTO;
import org.ecews.renaissance.repositories.OphthalmologistRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class OphthalmologistService {
    
    private final OphthalmologistRepository repository;
    private final Utils utils;
    
    public Ophthalmologist save(OphthalmologistDTO dto) {
        Ophthalmologist o = new Ophthalmologist();
        o.setId(dto.getId());
        o.setPatientId(dto.getPatientId());
        o.setReferred(dto.getReferred());
        o.setDiagnoses(dto.getDiagnoses());
        o.setTreatments(dto.getTreatments());
        o.setSurgeries(dto.getSurgeries());
        o.setCreatedBy(utils.getCurrentUserName());
        o.setCreatedDate(LocalDateTime.now());
        o.setUpdatedDate(LocalDateTime.now());
        o.setUpdatedBy(utils.getCurrentUserName());
        o.setOthersDiagnosis(dto.getOthersDiagnosis());
        o.setOthersTreatment(dto.getOthersTreatment());
        o.setOtherSurgery(dto.getOtherSurgery());
        o.setVisualAcuityLeft(dto.getVisualAcuityLeft());
        o.setVisualAcuityRight(dto.getVisualAcuityRight());
        o.setGlassesDispensed(dto.getGlassesDispensed());
        
        return repository.save(o);
    }
    
    public List<Ophthalmologist> saveAll(List<OphthalmologistDTO> dtos) {
        return dtos.stream().map(this::save).toList();
    }
    
    public Ophthalmologist update(UUID id, OphthalmologistDTO dto) {
        Ophthalmologist o = repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Ophthalmologist record not found"));
        
        o.setDiagnoses(dto.getDiagnoses());
        o.setTreatments(dto.getTreatments());
        o.setSurgeries(dto.getSurgeries());
        o.setReferred(dto.getReferred());
        o.setUpdatedBy(utils.getCurrentUserName());
        o.setUpdatedDate(LocalDateTime.now());
        o.setOthersDiagnosis(dto.getOthersDiagnosis());
        o.setOthersTreatment(dto.getOthersTreatment());
        o.setOtherSurgery(dto.getOtherSurgery());
        o.setVisualAcuityLeft(dto.getVisualAcuityLeft());
        o.setVisualAcuityRight(dto.getVisualAcuityRight());
        o.setGlassesDispensed(dto.getGlassesDispensed());
        
        return repository.save(o);
    }
    
    public void delete(UUID id) {
        Ophthalmologist o = repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Ophthalmologist record not found"));
        o.setArchived(true);
        repository.save(o);
    }
    
    public List<Ophthalmologist> findByPatientId(UUID patientId) {
        try {
            return repository.findByPatientId(patientId);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
    
    public Ophthalmologist findById(UUID id) {
        return repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Ophthalmologist record not found"));
    }
    
    public List<Ophthalmologist> findAll() {
        return repository.findAll().stream()
                .filter(p -> !p.isArchived())
                .toList();
    }
}
